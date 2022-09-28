using Npgsql;
using Dapper;
using Domain.Dtos;
using Domain.Wrapper;
using Infrastructure.DataContext;
using Microsoft.AspNetCore.Hosting;
using Npgsql.Internal.TypeHandlers.GeometricHandlers;

public class QuoteService
{
    private DataContext _context;
    private readonly IWebHostEnvironment _environment;

    public QuoteService(DataContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<Response<List<Quote>>> GetQuotes()
    {
        await using var connection = _context.CreateConnection();
        var sql = "select  * from quote";
        var result = await connection.QueryAsync<Quote>(sql);
        return new Response<List<Quote>>(result.ToList());
    }

    public async Task<Response<List<QuoteCategoryDto>>> GetQuoteWithCategory()
    {
        await using var connection = _context.CreateConnection();
        var sql =
            "select q.id, q.Author, q.QuoteText, c.CategoryName  from quote as q join category as c on c.id = q.categoryid;";
        var result = await connection.QueryAsync<QuoteCategoryDto>(sql);
        return new Response<List<QuoteCategoryDto>>(result.ToList());
    }

    public async Task<Response<GetQuoteDto>> AddQuote(CreateQuoteDto quote)
    {
        using var connection = _context.CreateConnection();
        try
        {
            var path  = Path.Combine(_environment.WebRootPath, "images", quote.QuoteImageFile.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            await quote.QuoteImageFile.CopyToAsync(stream);
            
            var sql =
                "insert into quotes (Author, QuoteText, QuoteImage, CategoryId) values (@Author, @QuoteText,@QuoteImage,@CategoryId) returning id;";
            var result =
                await connection.ExecuteScalarAsync<int>(sql,
                    new { quote.Author, quote.QuoteText, QuoteImage = quote.QuoteImageFile.FileName, quote.CategoryId });
            quote.Id = result;
            var getQuote = new GetQuoteDto
            {
                Id = quote.Id,
                Author = quote.Author,
                QuoteText = quote.QuoteText,
                QuoteImage = quote.QuoteImageFile.FileName,
                CategoryId = quote.CategoryId
            };
            return new Response<GetQuoteDto>(getQuote);
        }
        catch (Exception ex)
        {
            return new Response<GetQuoteDto>(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}