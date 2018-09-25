using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext context;

        public DatingRepository(DataContext context)
        {
            this.context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await context.Likes.FirstOrDefaultAsync(user => user.LikerId == userId && user.LikeeId == recipientId);

        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await context.Users
            .Include(p => p.Photos)
            .Include(l => l.Likers)
            .Include(l => l.Likees)
            .FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PageList<User>> GetUsers(UserParams userParams)
        {
            var users = context.Users
            .Include(p => p.Photos)
            .OrderByDescending(u => u.LastActive)
            .AsQueryable();

            var likes = context.Likes.AsQueryable();

            users = users.Where(user => user.Id != userParams.UserId);
            users = users.Where(user => user.Gender == userParams.Gender);
            
            if (userParams.Likers)
            {
                likes = likes.Where(like => like.LikeeId==userParams.UserId);
                users = likes.Select(like => like.Liker);
            }

            if (userParams.Likees)
            {
                likes = likes.Where(like => like.LikerId==userParams.UserId);
                users = likes.Select(like => like.Likee);
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDoB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDoB = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(user => user.DateOfBirth >= minDoB && user.DateOfBirth <= maxDoB);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            return await PageList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}