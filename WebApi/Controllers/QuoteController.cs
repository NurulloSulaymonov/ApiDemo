using Domain.Wrapper;
using Infrastructure.DataContext;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class QuoteController : ControllerBase
{
    private QuoteService _quoteService;
    public QuoteController(QuoteService quoteService)
    {
        _quoteService = quoteService;
    }
  

    [HttpGet("getquotes")]
    public async Task<Response<List<Quote>>> GetQuotes()
    {
        return await _quoteService.GetQuotes();
    }

    [HttpGet("getquotewithCategory")]
    public async Task<Response<List<QuoteCategoryDto>>> GetQuoteWithCategory()
    {
        return await _quoteService.GetQuoteWithCategory();
    }


    [HttpPost("AddQuote")]
    public async Task<Response<Quote>> AddQuote(Quote quote)
    {
        return await _quoteService.AddQuote(quote);
    }

}