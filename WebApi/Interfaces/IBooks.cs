using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Interfaces
{
    public interface IBooks
    {
        List<Books> GetAll();
        Books GetById(int id);
        Books DeleteById(int id);
        void UpdateBook(Books request);
        void AddBook(Books book);
    }
}
