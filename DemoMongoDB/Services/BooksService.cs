using DemoMongoDB.Dto;
using DemoMongoDB.Entities;
using DemoMongoDB.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Reflection;
using System.Xml.Linq;

namespace DemoMongoDB.Services
{
    public class BooksService
    {
        private readonly IMongoCollection<Book> _booksCollection;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoDatabase _bookStoreDb;

        public BooksService(
            IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<Book>(
                bookStoreDatabaseSettings.Value.BooksCollectionName);
            _usersCollection = mongoDatabase.GetCollection<User>(
                bookStoreDatabaseSettings.Value.UsersCollectionName);

            _bookStoreDb = mongoDatabase;
        }

        public async Task<List<Book>> GetAsync(PageResultBookDto input)
        {
            // search
            var filter = new BsonDocument { };
            if (input.Keyword != null)
            {
                filter.Add(new BsonElement("Name", new BsonDocument { { "$regex", input.Keyword }, { "$options", "i" } }));
            }
            if (input.Category != null)
            {
                filter.Add(new BsonElement("Category", input.Category));
            }
            return await _booksCollection.Find(filter).Skip(input.Skip).Limit(input.PageSize).ToListAsync();
        }
        public async Task<List<Book>> GetAsyncWithFilter(int pageNumber, int? pageSize, string filterEqual, string filterLike)
        {
            var filter = new BsonDocument { };
            // Dynamic filter: Query theo kiểu tuyệt đối (equal) hoặc tương đối (like)
            if (filterEqual != null)
            {
                AddEqualFilterBson(filterEqual, filter);
            }
            if (filterLike != null)
            {
                AddLikeFilterBson(filterLike, filter);
            }

            return await _booksCollection.Find(filter).Skip(pageSize * pageNumber).Limit(pageSize).ToListAsync();
        }

        public string CreateJsonParams(CreateJsonParamsDto input)
        {
            return JsonConvert.SerializeObject(new 
            {
                Category = input.category,
                Author = input.author
            });
        }

        public List<Book> GetAsyncLinQ(PageResultBookDto input)
        {
            List<Book> result = (from book in _booksCollection.AsQueryable()
            select new Book 
            { 
                Id = book.Id, 
                BookName = book.BookName, 
                Price = book.Price, 
                Category = book.Category, 
                Author = book.Author 
            })
            .Skip(input.Skip).Take(input.PageSize).ToList();
            if (input.Keyword != null)
            {
                result = result.Where(a => a.BookName.ToLower().Contains(input.Keyword.ToLower())).ToList();
            }
            if (input.Category != null)
            {
                result = result.Where(c => c.Category == input.Category).ToList();
            }
            return result;
        }
        
        public async Task<Book?> GetAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Book newBook) =>
            await _booksCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, Book updatedBook) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);

        // Them collection User
        public async Task CreateUserAsync(User input) =>
            await _usersCollection.InsertOneAsync(input);
        
        public async Task<List<object>> GetUserAsync()
        {
            var result = await _bookStoreDb.GetCollection<object>("Users").Find(_ => true).ToListAsync();
            return result;
        }

        public BsonDocument AddEqualFilterBson(string filterJson, BsonDocument filter)
        {
            char[] charsToTrim = { '\"', '{', '}', '\\', ' ' };
            string[] objItem = filterJson.Trim(charsToTrim).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in objItem)
            {
                string[] el = value.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var key = el[0].Trim(charsToTrim);
                var val = el[1].Trim(charsToTrim);
                filter.Add(new BsonElement(key, val));
            }
            return filter;
        }

        public BsonDocument AddLikeFilterBson(string filterJson, BsonDocument filter)
        {
            char[] charsToTrim = { '\"', '{', '}', '\\', ' ' };
            string[] objItem = filterJson.Trim(charsToTrim).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in objItem)
            {
                string[] el = value.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var key = el[0].Trim(charsToTrim);
                var val = el[1].Trim(charsToTrim);
                filter.Add(new BsonElement(key, new BsonDocument { { "$regex", val }, { "$options", "i" } }));
            }
            return filter;
        }
    }
}
