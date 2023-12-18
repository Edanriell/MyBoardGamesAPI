using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace MyBGList.Models
{
    [Table("BoardGames")]
    public class BoardGame
    {
        [Key]
        [Required]
        public int Id { get; set; }

        // One To Many ForeignKey for reference
        // [Required]
        // public int PublisherId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        [Required]
        public int MinPlayers { get; set; }

        [Required]
        public int MaxPlayers { get; set; }

        [Required]
        public int PlayTime { get; set; }

        [Required]
        public int MinAge { get; set; }

        [Required]
        public int UsersRated { get; set; }

        [Required]
        [Precision(4, 2)]
        public decimal RatingAverage { get; set; }

        [Required]
        public int BGGRank { get; set; }

        [Required]
        [Precision(4, 2)]
        public decimal ComplexityAverage { get; set; }

        [Required]
        public int OwnedUsers { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastModifiedDate { get; set; }

        // [MaxLength(200)]
        // public string AlternateNames { get; set; } = null!;

        // [MaxLength(200)]
        // public string Designer { get; set; } = null!;

        // [Required]
        // public int Flags { get; set; }

        public ICollection<BoardGames_Domains>? BoardGames_Domains { get; set; }

        public ICollection<BoardGames_Mechanics>? BoardGames_Mechanics { get; set; }

        // Many To Many
        // public ICollection<BoardGames_Categories>? BoardGames_Categories { get; set; }

        // One To Many (One BoardGame one publisher)
        // public Publisher? Publisher { get; set; }
    }
}
