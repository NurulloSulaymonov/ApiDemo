using Npgsql;
using Dapper;
using Domain.Dtos;
using Domain.Wrapper;
using Infrastructure.DataContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        var sql = "select  * from quotes";
        var result = await connection.QueryAsync<Quote>(sql);
        foreach (var quote in result)
        {
           var images = await GetImages(quote.Id);
              quote.QuoteImages = images;
        }
        return new Response<List<Quote>>(result.ToList());
    }
    
    private async Task<List<string>> GetImages(int quoteId)
    {
        await using var connection = _context.CreateConnection();
        var sql = "select q.imagename  from quoteimages q  where quoteid = @quoteId";
        var result = await connection.QueryAsync<string>(sql, new {quoteId});
        return result.ToList();
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
            // insert the quote and get the id of the inserted quote
            var sql =
                "insert into quotes (Author, QuoteText, CategoryId) values (@Author, @QuoteText,@CategoryId) returning id;";
            var result = await connection.ExecuteScalarAsync<int>(sql,
                    new { quote.Author, quote.QuoteText, quote.CategoryId });
            quote.Id = result;
            
            await  InsertImages(quote.QuoteImageFiles, quote.Id);   
           
            var getQuote = new GetQuoteDto
            {
                Id = quote.Id,
                Author = quote.Author,
                QuoteText = quote.QuoteText,
                CategoryId = quote.CategoryId
            };
            return new Response<GetQuoteDto>(getQuote);
        }
        catch (Exception ex)
        {
            return new Response<GetQuoteDto>(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private async  Task InsertImages(List<IFormFile> images, int quoteid)
    {
        foreach (var item in images)
        {
            var path  = Path.Combine(_environment.WebRootPath, "images", item.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            await item.CopyToAsync(stream);
            
            //insert into database
            using var connection = _context.CreateConnection();
            var sql = "insert into quoteimages (QuoteId, imagename) values (@QuoteId, @ImageName);";
            await connection.ExecuteAsync(sql, new {QuoteId = quoteid, ImageName = item.FileName});
        }   
    }
}