using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.Attributes;
using MyBGList.DTO;
using MyBGList.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using MyBGList.Constants;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace MyBGList.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BoardGamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<BoardGamesController> _logger;

        private readonly IMemoryCache _memoryCache;

        public BoardGamesController(
            ApplicationDbContext context,
            ILogger<BoardGamesController> logger,
            IMemoryCache memoryCache
        )
        {
            _context = context;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        /*        [HttpGet(Name = "GetBoardGames")]
                [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
                */
        /*        public IEnumerable<BoardGame> Get()
                        {
                            return new[]
                            {
                                new BoardGame()
                                {
                                    Id = 1,
                                    Name = "Axis & Allies",
                                    Year = 1981,
                                    MinPlayers = 2,
                                    MaxPlayers = 5,
                                },
                                new BoardGame()
                                {
                                    Id = 2,
                                    Name = "Citadels",
                                    Year = 2000,
                                    MinPlayers = 2,
                                    MaxPlayers = 8,
                                },
                                new BoardGame()
                                {
                                    Id = 3,
                                    Name = "Terraforming Mars",
                                    Year = 2016,
                                    MinPlayers = 1,
                                    MaxPlayers = 5,
                                }
                            };
                        }*/
        /*
                public async Task<RestDTO<BoardGame[]>> Get()
                {
                    var query = _context.BoardGames;
        
                    return new RestDTO<BoardGame[]>()
                    {
                        Data = await query.ToArrayAsync(),
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(
                                Url.Action(null, "BoardGames", null, Request.Scheme)!,
                                "selft",
                                "GET"
                            ),
                        }
                    };
                    */
        /*            return new RestDTO<BoardGame[]>()
                                {
                                    Data = new BoardGame[]
                                    {
                                        new BoardGame()
                                        {
                                            Id = 1,
                                            Name = "Axis & Allies",
                                            Year = 1981
                                        },
                                        new BoardGame()
                                        {
                                            Id = 2,
                                            Name = "Citadels",
                                            Year = 2000
                                        },
                                        new BoardGame()
                                        {
                                            Id = 3,
                                            Name = "Terraforming Mars",
                                            Year = 2016
                                        }
                                    },
                                    Links = new List<LinkDTO>
                                    {
                                        new LinkDTO(
                                            Url.Action(null, "BoardGames", null, Request.Scheme)!,
                                            "self",
                                            "GET"
                                        ),
                                    }
                                };*/
        /*
                }*/
        /*        [HttpGet(Name = "GetBoardGames")]
                [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
                public async Task<RestDTO<BoardGame[]>> Get(
                    int pageIndex = 0,
                    int pageSize = 10,
                    string? sortColumn = "Name",
                    string? sortOrder = "ASC",
                    string? filterQuery = null
                )
                {
                    var query = _context.BoardGames.AsQueryable();
                    if (!string.IsNullOrEmpty(filterQuery))
                        query = query.Where(b => b.Name.Contains(filterQuery));
                    var recordCount = await query.CountAsync();
                    query = query
                        .OrderBy($"{sortColumn} {sortOrder}")
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize);
        
                    return new RestDTO<BoardGame[]>()
                    {
                        Data = await query.ToArrayAsync(),
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        RecordCount = recordCount,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(
                                Url.Action(
                                    null,
                                    "BoardGames",
                                    new { pageIndex, pageSize },
                                    Request.Scheme
                                )!,
                                "self",
                                "GET"
                            ),
                        }
                    };
                }*/
        // [Authorize]
        [HttpGet(Name = "GetBoardGames")]
        /*[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]*/
        // [ResponseCache(CacheProfileName = "Any-60")]
        [ResponseCache(CacheProfileName = "Client-120")]
        [SwaggerOperation(
            Summary = "Get a list of board games.",
            Description = "Retrieves a list of board games "
                + "with custom paging, sorting, and filtering rules."
        )]
        public async Task<RestDTO<BoardGame[]>> Get(
            // int pageIndex = 0,
            // [Range(1, 100)] int pageSize = 10,
            // [SortColumnValidator(typeof(BoardGameDTO))] string? sortColumn = "Name",
            // [RegularExpression("ASC|DESC")] string? sortOrder = "ASC",
            /*            [SortOrderValidator(ErrorMessage = "Custom error message")]
                            string? sortOrder = "ASC",*/
            /*            [SortOrderValidator(AllowedValues = new[] { "ASC", "DESC", "OtherString" })]
                            string? sortOrder = "ASC",*/
            /*            [SortOrderValidator]
                            string? sortOrder = "ASC",*/
            // string? filterQuery = null
            [FromQuery]
            [SwaggerParameter(
                "A DTO object that can be used " + "to customize some retrieval parameters."
            )]
                RequestDTO<BoardGameDTO> input
        )
        {
            // _logger.LogInformation(50110, "Get method started.");
            /*            _logger.LogInformation(CustomLogEvents.BoardGamesController_Get, "Get method started.");

                        var query = _context.BoardGames.AsQueryable();
                        */
            /*            if (!string.IsNullOrEmpty(filterQuery))
                                                    query = query.Where(b => b.Name.StartsWith(filterQuery));*/
            /*
                                    if (!string.IsNullOrEmpty(input.FilterQuery))
                                        query = query.Where(b => b.Name.Contains(input.FilterQuery));
            
                                    var recordCount = await query.CountAsync();
            
                                    BoardGame[]? result = null;
            
                                    var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";
            
                                    if (!_memoryCache.TryGetValue<BoardGame[]>(cacheKey, out result))
                                    {
                                        query = query
                                            .OrderBy($"{input.SortColumn} {input.SortOrder}")
                                            .Skip(input.PageIndex * input.PageSize)
                                            .Take(input.PageSize);
                                        result = await query.ToArrayAsync();
                                        _memoryCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
                                    }*/

            (int recordCount, BoardGame[]? result) dataTuple = (0, null);

            var cacheKey = $"{input.GetType()}-{JsonSerializer.Serialize(input)}";

            if (!_memoryCache.TryGetValue(cacheKey, out dataTuple))
            {
                var query = _context.BoardGames.AsQueryable();

                if (!string.IsNullOrEmpty(input.FilterQuery))
                    query = query.Where(b => b.Name.Contains(input.FilterQuery));

                dataTuple.recordCount = await query.CountAsync();

                query = query
                    .OrderBy($"{input.SortColumn} {input.SortOrder}")
                    .Skip(input.PageIndex * input.PageSize)
                    .Take(input.PageSize);

                dataTuple.result = await query.ToArrayAsync();

                _memoryCache.Set(cacheKey, dataTuple, new TimeSpan(0, 0, 30));
            }

            /*            query = query
                            .OrderBy($"{sortColumn} {sortOrder}")
                            .Skip(pageIndex * pageSize)
                            .Take(pageSize);*/
            /*            query = query
                            .OrderBy($"{input.SortColumn} {input.SortOrder}")
                            .Skip(input.PageIndex * input.PageSize)
                            .Take(input.PageSize);*/

            /*            return new RestDTO<BoardGame[]>()
                        {
                            Data = await query.ToArrayAsync(),
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            RecordCount = recordCount,
                            Links = new List<LinkDTO>
                            {
                                new LinkDTO(
                                    Url.Action(
                                        null,
                                        "BoardGames",
                                        new { pageIndex, pageSize },
                                        Request.Scheme
                                    )!,
                                    "self",
                                    "GET"
                                ),
                            }
                        };*/

            /*            return new RestDTO<BoardGame[]>()
                        {
                            // Data = await query.ToArrayAsync(),
                            Data = result!,
                            PageIndex = input.PageIndex,
                            PageSize = input.PageSize,
                            RecordCount = recordCount,
                            Links = new List<LinkDTO>
                            {
                                new LinkDTO(
                                    Url.Action(
                                        null,
                                        "BoardGames",
                                        new { input.PageIndex, input.PageSize },
                                        Request.Scheme
                                    )!,
                                    "self",
                                    "GET"
                                ),
                            }
                        };*/

            return new RestDTO<BoardGame[]>()
            {
                Data = dataTuple.result!,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = dataTuple.recordCount,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "BoardGames",
                            new { input.PageIndex, input.PageSize },
                            Request.Scheme
                        )!,
                        "self",
                        "GET"
                    ),
                }
            };
        }

        [HttpGet("{id}")]
        [ResponseCache(CacheProfileName = "Any-60")]
        [SwaggerOperation(
            Summary = "Get a single board game.",
            Description = "Retrieves a single board game with the given Id."
        )]
        public async Task<RestDTO<BoardGame?>> Get([CustomKeyValue("x-test-3", "value 3")] int id)
        {
            _logger.LogInformation(
                CustomLogEvents.BoardGamesController_Get,
                "GetBoardGame method started."
            );

            BoardGame? result = null;
            var cacheKey = $"GetBoardGame-{id}";
            if (!_memoryCache.TryGetValue<BoardGame>(cacheKey, out result))
            {
                result = await _context.BoardGames.FirstOrDefaultAsync(bg => bg.Id == id);
                _memoryCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
            }

            return new RestDTO<BoardGame?>()
            {
                Data = result,
                PageIndex = 0,
                PageSize = 1,
                RecordCount = result != null ? 1 : 0,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", new { id }, Request.Scheme)!,
                        "self",
                        "GET"
                    ),
                }
            };
        }

        // [Authorize]
        [Authorize(Roles = RoleNames.Moderator)]
        [HttpPost(Name = "UpdateBoardGame")]
        /*[ResponseCache(NoStore = true)]*/
        [ResponseCache(CacheProfileName = "NoCache")]
        [SwaggerOperation(
            Summary = "Updates a board game.",
            Description = "Updates the board game's data."
        )]
        public async Task<RestDTO<BoardGame?>> Post(BoardGameDTO bgDTO)
        {
            var boardgame = await _context.BoardGames
                .Where(b => b.Id == bgDTO.Id)
                .FirstOrDefaultAsync();
            if (boardgame != null)
            {
                if (!string.IsNullOrEmpty(bgDTO.Name))
                    boardgame.Name = bgDTO.Name;
                if (bgDTO.Year.HasValue && bgDTO.Year.Value > 0)
                    boardgame.Year = bgDTO.Year.Value;
                if (bgDTO.MinPlayers.HasValue && bgDTO.MinPlayers.Value > 0)
                    boardgame.MinPlayers = bgDTO.MinPlayers.Value;
                if (bgDTO.MaxPlayers.HasValue && bgDTO.MaxPlayers.Value > 0)
                    boardgame.MaxPlayers = bgDTO.MaxPlayers.Value;
                if (bgDTO.PlayTime.HasValue && bgDTO.PlayTime.Value > 0)
                    boardgame.PlayTime = bgDTO.PlayTime.Value;
                if (bgDTO.MinAge.HasValue && bgDTO.MinAge.Value > 0)
                    boardgame.MinAge = bgDTO.MinAge.Value;
                boardgame.LastModifiedDate = DateTime.Now;
                _context.BoardGames.Update(boardgame);
                await _context.SaveChangesAsync();
            }
            ;

            return new RestDTO<BoardGame?>()
            {
                Data = boardgame,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", bgDTO, Request.Scheme)!,
                        "self",
                        "POST"
                    ),
                }
            };
        }

        // [Authorize]
        [Authorize(Roles = RoleNames.Administrator)]
        [HttpDelete(Name = "DeleteBoardGame")]
        /*[ResponseCache(NoStore = true)]*/
        [ResponseCache(CacheProfileName = "NoCache")]
        [SwaggerOperation(
            Summary = "Deletes a board game.",
            Description = "Deletes a board game from the database."
        )]
        public async Task<RestDTO<BoardGame[]?>> Delete(string ids)
        {
            var idArray = ids.Split(',').Select(x => int.Parse(x));
            var deletedBGList = new List<BoardGame>();

            foreach (int id in idArray)
            {
                var boardgame = await _context.BoardGames
                    .Where(b => b.Id == id)
                    .FirstOrDefaultAsync();
                if (boardgame != null)
                {
                    deletedBGList.Add(boardgame);
                    _context.BoardGames.Remove(boardgame);
                    await _context.SaveChangesAsync();
                }
                ;
            }

            return new RestDTO<BoardGame[]?>()
            {
                Data = deletedBGList.Count > 0 ? deletedBGList.ToArray() : null,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", ids, Request.Scheme)!,
                        "self",
                        "DELETE"
                    ),
                }
            };
        }
    }
}
