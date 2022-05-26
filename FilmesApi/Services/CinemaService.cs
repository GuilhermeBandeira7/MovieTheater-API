using AutoMapper;
using FilmesApi.Data;
using FilmesAPI.Data.Dtos;
using FilmesAPI.Models;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmesApi.Services
{
    public class CinemaService
    {
        private IMapper _mapper;
        private AppDbContext _context;

        public CinemaService(IMapper mapper, AppDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public List<ReadCinemaDto> RecuperaCinemas(string nomeDoFilme) //Get method returning a list of Movies(Cinemas)
        {
            List<Cinema> cinemas = _context.Cinemas.ToList(); // listing all movies inside DbContext
            if (cinemas == null)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(nomeDoFilme))
            {
                // using a query structure to find the Movie name passed as argument
                IEnumerable<Cinema> query = from cinema in cinemas
                                            where cinema.Sessoes.Any(sessao =>
                                            sessao.Filme.Titulo == nomeDoFilme)
                                            select cinema;

                cinemas = query.ToList();
            }
            return _mapper.Map<List<ReadCinemaDto>>(cinemas); //mapping a movie to movieDto
        }

        public ReadCinemaDto AdicionaCinema(CreateCinemaDto cinemaDto)
        {
            Cinema cinema = _mapper.Map<Cinema>(cinemaDto); //mapping a moviedto to a movie object
            _context.Cinemas.Add(cinema); //adding the movie received as argument in the db
            _context.SaveChanges(); //saving the changes made 
            return _mapper.Map<ReadCinemaDto>(cinema); //mapping and returning a readmoviedto
        }

        public ReadCinemaDto RecuperaCinemasPorId(int id)
        {
            Cinema cinema = _context.Cinemas.FirstOrDefault(cinema => cinema.Id == id); //finding the id in the db
            if (cinema != null)
            {
                return _mapper.Map<ReadCinemaDto>(cinema); // if the id was found return it 
            }
            return null;
        }

        public Result AtualizaCinema(int id, UpdateCinemaDto cinemaDto)
        {
            Cinema cinema = _context.Cinemas.FirstOrDefault(cinema => cinema.Id == id);
            if (cinema == null)
            {
                return Result.Fail("Cinema não encontrado");
            }
            _mapper.Map(cinemaDto, cinema);
            _context.SaveChanges();
            return Result.Ok();
        }

        public Result DeletaCinema(int id)
        {
            Cinema cinema = _context.Cinemas.FirstOrDefault(cinema => cinema.Id == id);
            if (cinema == null)
            {
                return Result.Fail("Cinema não encontrado");
            }
            _context.Remove(cinema);
            _context.SaveChanges();
            return Result.Ok();
        }
    }
}
