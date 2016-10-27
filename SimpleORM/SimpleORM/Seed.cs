namespace SimpleORM
{
    using SimpleORM.Contracts;
    using SimpleORM.Entities;
    using SimpleORM.Core;
    using System;
    using System.Collections.Generic;
    public class Seed
    {
        public static void Main()
        {
            ConnectionStringBuilder db = new ConnectionStringBuilder("SimpleORMDB");
            IDbContext dbManager = new EntityManager(db.ConnectionString, true);

            #region Code-First approach
            //User user = new User("Bobby", "b0dd1", 34, System.DateTime.Now, System.DateTime.Now, true);
            //dbManager.Persist(user);
            //Console.WriteLine(user);
            #endregion

            #region Update Entity's details
            //User user = dbManager.FindById<User>(1);
            //user.Password = "newDragon123";
            //user.Username = "Stefchu";
            //dbManager.Persist(user);
            //Console.WriteLine(user);
            #endregion

            #region Test ORM Methods returning single values
            //User user = dbManager.FindFirst<User>();
            //User user = dbManager.FindById<User>(1);
            //User user = dbManager.FindFirst<User>("username = 'stefchu'");
            //Console.WriteLine(user);
            #endregion

            #region Test ORM Methods returning multiple values
            //User user = new User("Dancho", "pass", 25, System.DateTime.Now, System.DateTime.Now, true);
            //User anotherUser = new User("Vyara", "sun", 22, System.DateTime.Now, System.DateTime.Now, true);
            //dbManager.Persist(user);
            //dbManager.Persist(anotherUser);

            //IEnumerable<User> users = dbManager.FindAll<User>();
            //IEnumerable<User> users = dbManager.FindAll<User>("Id=2");

            //foreach (var usr in users)
            //{
            //    Console.WriteLine(usr);
            //}
            #endregion

            #region Test Delete Methods
            //User user = dbManager.FindById<User>(1);
            //dbManager.Delete<User>(user);
            #endregion

            #region Create new Entity and use FindAll
            //List<Book> books = new List<Book>()
            //{
            //    new Book("Harry Potter and the Cursed Child - Parts I & II", "J.K. Rowling , Jack Thorne , John Tiffany", new DateTime(2015, 10, 2), "English", true, 1),
            //    new Book("Merchant of Venice", "Shakespeare W.", new DateTime(2013, 11, 3), "English", false, 2.3m),
            //    new Book("Short Stories from the Nineteenth Century", "Davies D.S.(Ed.)", new DateTime(2011, 12, 4), "English", false, 4),
            //    new Book("The Horror in the Museum: Collected Short Stories Vol.2", "Lovecraft H.P.", new DateTime(2004, 1, 5), "English", true, 6.4m),
            //    new Book("Twenty Thousand Leagues Under the Sea", "Verne J.", new DateTime(2042, 7, 6), "English", false, 7),
            //    new Book("Mansfield Park", "Austen J.", new DateTime(2003, 6, 7), "English", true, 9.2m),
            //    new Book("Adventures & Memoirs of Sherlock Holmes", "Doyle A.C.", new DateTime(2023, 2, 8), "English", true, 10),
            //    new Book("Lord Jim", "Conrad J.", new DateTime(2052, 4, 9), "English", false, 8.2m),
            //    new Book("Three Musketeers", "Dumas A.", new DateTime(2012, 1, 30), "English", true, 5.2m),
            //    new Book("Tale of Two Cities", "Dickens C.", new DateTime(2005, 5, 21), "English", false, 2.1m),
            //};

            //foreach (Book book in books)
            //{
            //    dbManager.Persist(book);
            //}

            //Console.WriteLine("Enter a number:");
            //int lenghtOfTitle = int.Parse(Console.ReadLine());
            //IEnumerable<Book> wantedBooks = dbManager.FindAll<Book>($"LEN(Title) >= {lenghtOfTitle} AND IsHardCovered = 1");

            //int numberOfBooksWithGivenLength = 0;
            //foreach (Book book in wantedBooks)
            //{
            //    if (book.Title.Length >= lenghtOfTitle)
            //    {
            //        numberOfBooksWithGivenLength++;
            //    }
            //}

            //Console.WriteLine($"{numberOfBooksWithGivenLength} books have title with lenght of {lenghtOfTitle}");
            #endregion

            #region FindAll(string condition) and Delete
            //IEnumerable<Book> books = dbManager.FindAll<Book>("Rating < 2");
            //int deletedBooks = 0;
            //foreach (Book book in books)
            //{
            //    dbManager.Delete<Book>(book);
            //    Console.WriteLine(book.Title);
            //    deletedBooks++;
            //}
            //Console.WriteLine($"{deletedBooks} books have been deleted from the database");
            #endregion

        }
    }
}
