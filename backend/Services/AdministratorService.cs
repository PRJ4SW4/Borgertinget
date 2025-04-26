using System;
using System.Collections.Generic;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AdministratorService
    {
        private readonly DataContext _context;

        public AdministratorService(DataContext context)
        {
            _context = context;
        }

        // POST Flashcard collection
        public async Task<int> CreateCollectionAsync(FlashcardCollectionDetailDto dto)
        {
            var collection = new FlashcardCollection
            {
                Title = dto.Title,
                Description = dto.Description,
                DisplayOrder = 0,
            };

            foreach (var fc in dto.Flashcards)
            {
                // Try parse the string into an enum for frontType and backType
                Enum.TryParse<FlashcardContentType>(fc.FrontContentType, out var FrontType);
                Enum.TryParse<FlashcardContentType>(fc.BackContentType, out var BackType);

                var flashcard = new Flashcard
                {
                    FrontContentType = FrontType,
                    FrontText = fc.FrontText,
                    FrontImagePath = fc.FrontImagePath,
                    BackContentType = BackType,
                    BackText = fc.BackText,
                    BackImagePath = fc.BackImagePath,
                };
                // Add the flashcard to the collection
                collection.Flashcards.Add(flashcard);
            }

            // Save flashcart collection to the database
            _context.FlashcardCollections.Add(collection);
            await _context.SaveChangesAsync();

            // Returns the collection id
            return collection.CollectionId;
        }

        // PUT Flashcard Collection
        public async Task UpdateCollectionInfoAsync(
            int collectionId,
            FlashcardCollectionDetailDto dto
        )
        {
            // Get the specific Flashcard Collection with the flashcards too
            var collection = await _context
                .FlashcardCollections.Include(c => c.Flashcards)
                .FirstOrDefaultAsync(c => c.CollectionId == collectionId);

            if (collection == null)
            {
                throw new KeyNotFoundException(
                    $"Flashcard Collection with ID {collectionId} not found"
                );
            }

            // Update collection title and Description
            collection.Title = dto.Title;
            collection.Description = dto.Description;

            // Clear and replace all flashcards
            _context.Flashcards.RemoveRange(collection.Flashcards);

            foreach (var fc in dto.Flashcards)
            {
                // Try parse the string into an enum for frontType and backType
                Enum.TryParse<FlashcardContentType>(fc.FrontContentType, out var FrontType);
                Enum.TryParse<FlashcardContentType>(fc.BackContentType, out var BackType);

                var flashcard = new Flashcard
                {
                    FrontContentType = FrontType,
                    FrontText = fc.FrontText,
                    FrontImagePath = fc.FrontImagePath,
                    BackContentType = BackType,
                    BackText = fc.BackText,
                    BackImagePath = fc.BackImagePath,
                };
                // Add the flashcard to the collection
                collection.Flashcards.Add(flashcard);
            }

            await _context.SaveChangesAsync();
        }

        // Get List of Flashcard Collection Titles
        public async Task<List<string>> GetAllFlashcardCollectionTitlesAsync()
        {
            var titles = await _context.FlashcardCollections.Select(fc => fc.Title).ToListAsync();

            if (titles == null)
            {
                throw new KeyNotFoundException($"No Flashcard Collection titles found");
            }

            return titles;
        }

        // Get Flashcard collection by Title
        public async Task<FlashcardCollectionDetailDto> GetFlashCardCollectionByTitle(string title)
        {
            var collection = await _context
                .FlashcardCollections.Include(fc => fc.Flashcards)
                .FirstOrDefaultAsync(fc => fc.Title == title);

            if (collection == null)
            {
                throw new KeyNotFoundException(
                    $"Flashcard Collection with title: {title} not found"
                );
            }

            // Map entity to DTO
            var dto = new FlashcardCollectionDetailDto
            {
                CollectionId = collection.CollectionId,
                Title = collection.Title,
                Description = collection.Description,
                Flashcards = collection
                    .Flashcards.Select(fc => new FlashcardDto
                    {
                        FlashcardId = fc.FlashcardId,
                        FrontContentType = fc.FrontContentType.ToString(),
                        FrontText = fc.FrontText,
                        FrontImagePath = fc.FrontImagePath,
                        BackContentType = fc.BackContentType.ToString(),
                        BackText = fc.BackText,
                        BackImagePath = fc.BackImagePath,
                    })
                    .ToList(),
            };

            return dto;
        }

        // DELETE Flashcard collection
        public async Task DeleteFlashcardCollectionAsync(int collectionId)
        {
            var collection = await _context
                .FlashcardCollections.Include(c => c.Flashcards)
                .FirstOrDefaultAsync();

            if (collection == null)
            {
                throw new KeyNotFoundException($"User with ID {collectionId} not found");
            }

            // Remove flashcards in Flashcardcollection
            _context.Flashcards.RemoveRange(collection.Flashcards);

            // Remove FlashcardCollection
            _context.FlashcardCollections.Remove(collection);

            await _context.SaveChangesAsync();
        }

        // GET user by username
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            // Search user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {username} not found");
            }

            return user;
        }

        // Get all users
        public async Task<User[]> GetAllUsersAsync()
        {
            // Search user by username
            var users = await _context.Users.ToArrayAsync();

            if (users == null)
            {
                throw new KeyNotFoundException($"Error finding users");
            }

            return users;
        }

        // PUT changing a Users username
        public async Task UpdateUserNameAsync(int userId, UpdateUserNameDto dto)
        {
            // Search the user in the Db
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} is not found");
            }

            // Update username
            user.UserName = dto.UserName;

            // Save changes to the Db
            _context.SaveChanges();
        }
    }
}
