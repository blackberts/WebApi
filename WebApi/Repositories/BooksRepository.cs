using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Interfaces;
using WebApi.Models;

namespace WebApi.Repositories
{
    public class BooksRepository : IBooks
    {
        private readonly DataContext _dataContext;

        public BooksRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void AddBook(Books book)
        {
            _dataContext.Books.Add(book);
            _dataContext.SaveChanges();
        }

        public Books DeleteById(int id)
        {
            Books? book = _dataContext.Books.Find(id);

            if(book != null)
            {
                _dataContext.Books.Remove(book);
                _dataContext.SaveChanges();
                return book;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
        public List<Books> GetAll()
        {
            return _dataContext.Books.ToList();
        }

        public Books GetById(int id)
        {
            Books? book = _dataContext.Books.Find(id);

            if (book != null)
                return book;
            else
                throw new ArgumentNullException();
        }

        public void UpdateBook(Books request)
        {
            Books? book = _dataContext.Books.Find(request.Id);

            if(book != null)
            {
                book.Title = request.Title;
                book.Description = request.Description;
                book.Price = request.Price;
                book.TypeBook = request.TypeBook;

                _dataContext.SaveChanges();
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
    }
}
