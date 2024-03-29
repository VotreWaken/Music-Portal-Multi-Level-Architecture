﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultilingualSite.Filters;
using MusicPortal.BLL.Interfaces;
using MusicPortal.BLL.ModelsDTO;
using MusicPortal.Models.AccountModels;
using MusicPortal.Models.GenreModels;

namespace MusicPortal.Controllers
{
    [Culture]
    public class GenresController : Controller
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService repository)
        {
            this._genreService = repository;
        }

        public async Task<IActionResult> Create(GenreModel model)
        {
            GenreDTO genreDTO = new GenreDTO
            {
                Id = model.NewGenre.Id,
                Name = model.NewGenre.Name
            };
            await _genreService.Create(genreDTO);
            return RedirectToAction("Index", "Genres");
        }

        public async Task<IActionResult> Edit(int id)
        {

            GenreDTO genreDTO = await _genreService.GetById(id);


            if (genreDTO == null)
            {
                return NotFound();
            }


            GenreModel genreModel = new GenreModel
            {
                NewGenre = new Genre
                {
                    Id = genreDTO.Id,
                    Name = genreDTO.Name
                }
            };

            return View(genreModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Genre genre)
        {
            GenreDTO genreDTO = new GenreDTO
            {
                Id = genre.Id,
                Name = genre.Name
            };


            await _genreService.Update(genreDTO);


            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _genreService.Delete(id);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                var genres = await _genreService.GetAll();
                var genreDTOs = new List<GenreDTO>();

                foreach (var genre in genres)
                {
                    var genreDTO = new GenreDTO
                    {
                        Id = genre.Id,
                        Name = genre.Name
                    };

                    genreDTOs.Add(genreDTO);
                }

                GenreModel model = new GenreModel();
                model.Genres = genreDTOs;

                return View(model);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}

