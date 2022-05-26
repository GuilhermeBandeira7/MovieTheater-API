using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using UsuariosApi.Data.Dtos.Usuario;
using UsuariosApi.Data.Requests;
using UsuariosApi.Models;

namespace UsuariosApi.Services
{
    public class CadastroService //class created to manage user's registry
    {

        private IMapper _mapper;
        private UserManager<CustomIdentityUser> _userManager; //UserManager provides API's to manage a IdentityUser
        private EmailService _emailService; 

        public CadastroService(IMapper mapper,
            UserManager<CustomIdentityUser> userManager,
            EmailService emailService, RoleManager<IdentityRole<int>> roleManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
        }

        public Result CadastraUsuario(CreateUsuarioDto createDto) // Method to register a user
        {
            Usuario usuario = _mapper.Map<Usuario>(createDto);
            CustomIdentityUser usuarioIdentity = _mapper.Map<CustomIdentityUser>(usuario);
            Task<IdentityResult> resultadoIdentity = _userManager
                .CreateAsync(usuarioIdentity, createDto.Password);
            _userManager.AddToRoleAsync(usuarioIdentity, "regular");
            if (resultadoIdentity.Result.Succeeded) // if the registration succeed we send a token to the user's email 
            {
                var code = _userManager
                    .GenerateEmailConfirmationTokenAsync(usuarioIdentity).Result;
                var encodedCode = HttpUtility.UrlEncode(code); //encoding url

                _emailService.EnviarEmail(new[] { usuarioIdentity.Email },
                    "Link de Ativação", usuarioIdentity.Id, encodedCode);

                return Result.Ok().WithSuccess(code);
            }
            return Result.Fail("Falha ao cadastrar usuário");

        }

        public Result AtivaContaUsuario(AtivaContaRequest request) // method to activate a user's account
        {
            var identityUser = _userManager
                .Users
                .FirstOrDefault(u => u.Id == request.UsuarioId);
            var identityResult = _userManager
                .ConfirmEmailAsync(identityUser, request.CodigoDeAtivacao).Result;
            if (identityResult.Succeeded)
            {
                return Result.Ok();
            }
            return Result.Fail("Falha ao ativar conta de usuário");
        }
    }
}
