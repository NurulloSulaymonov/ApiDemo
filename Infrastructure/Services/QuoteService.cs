using Npgsql;
using Dapper;
using Domain.Wrapper;
public class QuoteService
{
    string _connectionString;
    public QuoteService()
    {
        _connectionString = "Server=127.0.0.1;Port=5432;Database=quotedb;User Id=postgres;Password=12345;";
    }

    public async Task<Response<List<Quote>>> GetQuotes()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            var sql = "select  * from quote";
            var result = await connection.QueryAsync<Quote>(sql);
            return new Response<List<Quote>>(result.ToList());
        }
    }

    public async Task<Response<List<QuoteCategoryDto>>> GetQuoteWithCategory()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            var sql = "select q.id, q.Author, q.QuoteText, c.CategoryName  from quote as q join category as c on c.id = q.categoryid;";
            var result = await connection.QueryAsync<QuoteCategoryDto>(sql);
            return new Response<List<QuoteCategoryDto>>(result.ToList());

        }
    }

    public async Task<Response<Quote>> AddQuote(Quote quote)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            try
            {
                var sql = "insert into quote (Author, QuoteText, CategoryId) values (@Author, @QuoteText, @CategoryId) returning id;";
                var result = await connection.ExecuteScalarAsync<int>(sql, new { quote.Author, quote.QuoteText, quote.CategoryId });
                quote.Id = result;
                return new Response<Quote>(quote);
            }
            catch (Exception ex)
            {
                return new Response<Quote>(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }

        }
    }

}