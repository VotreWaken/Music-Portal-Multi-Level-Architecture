﻿using Microsoft.EntityFrameworkCore;
using MusicPortal.DAL.Context;
using MusicPortal.DAL.Entities;
using MusicPortal.DAL.Interfaces;

namespace MusicPortal.DAL.Repositories
{
    public class AudioGenreRepository : IAudioGenre
    {
        // Context
        private readonly UserContext _context;

        // Constructor
        public AudioGenreRepository(UserContext context)
        {
            _context = context;
        }

        // Get All Audio Genres
        public async Task<List<AudioGenre>> GetAll()
        {
            return await _context.AudioGenre.ToListAsync();
        }

        // Get All Audio Genres By Id
        public async Task<AudioGenre> GetById(int audioId, int genreId)
        {
            return await _context.AudioGenre.FindAsync(audioId, genreId);
        }

        // Create Audio Genres
        public async Task<AudioGenre> Create(AudioGenre audioGenre)
        {
            _context.AudioGenre.Add(audioGenre);
            await _context.SaveChangesAsync();
            return audioGenre;
        }

        // Update Audio Genres
        public async Task Update(AudioGenre audioGenre)
        {
            _context.Entry(audioGenre).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Delete Audio Genres
        public async Task Delete(int audioId, int genreId)
        {
            var audioGenre = await _context.AudioGenre.FindAsync(audioId, genreId);
            if (audioGenre != null)
            {
                _context.AudioGenre.Remove(audioGenre);
                await _context.SaveChangesAsync();
            }
        }

        // Get Genre By Song
        public async Task<Dictionary<int, List<Genre>>> GetGenreBySong(IEnumerable<int> audioIds)
        {
            var genresByAudios = new Dictionary<int, List<Genre>>();

            foreach (var audioId in audioIds)
            {
                var genres = await _context.AudioGenre
                    .Where(ag => ag.AudioId == audioId)
                    .Include(ag => ag.Genre)
                    .Select(ag => new Genre
                    {
                        Id = ag.Genre.Id,
                        Name = ag.Genre.Name
                        // Другие свойства жанра, если есть
                    })
                    .ToListAsync();

                genresByAudios.Add(audioId, genres);
            }

            return genresByAudios;
        }

    }
}
