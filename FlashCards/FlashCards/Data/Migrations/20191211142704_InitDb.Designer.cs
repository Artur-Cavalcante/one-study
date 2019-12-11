﻿// <auto-generated />
using FlashCards.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FlashCards.Migrations
{
  [DbContext(typeof(DataContext))]
  [Migration("20191211142704_InitDb")]
  partial class InitDb
  {
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
      modelBuilder
          .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
          .HasAnnotation("ProductVersion", "3.0.0")
          .HasAnnotation("Relational:MaxIdentifierLength", 63);

      modelBuilder.Entity("FlashCards.Models.Card", b =>
          {
            b.Property<long>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnType("bigint")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<long>("DeckId")
                      .HasColumnType("bigint");

            b.Property<string>("Detail")
                      .HasColumnType("text");

            b.Property<string>("Title")
                      .HasColumnType("text");

            b.HasKey("Id");

            b.HasIndex("DeckId");

            b.ToTable("Cards");
          });

      modelBuilder.Entity("FlashCards.Models.Deck", b =>
          {
            b.Property<long>("Id")
                      .ValueGeneratedOnAdd()
                      .HasColumnType("bigint")
                      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            b.Property<string>("Title")
                      .HasColumnType("text");

            b.Property<string>("UserId")
                      .IsRequired()
                      .HasColumnType("text");

            b.HasKey("Id");

            b.HasIndex("UserId");

            b.ToTable("Decks");
          });

      modelBuilder.Entity("FlashCards.Models.User", b =>
          {
            b.Property<string>("Id")
                      .HasColumnType("text");

            b.HasKey("Id");

            b.ToTable("Users");
          });

      modelBuilder.Entity("FlashCards.Models.Card", b =>
          {
            b.HasOne("FlashCards.Models.Deck", "Deck")
                      .WithMany("Cards")
                      .HasForeignKey("DeckId")
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();
          });

      modelBuilder.Entity("FlashCards.Models.Deck", b =>
          {
            b.HasOne("FlashCards.Models.User", "User")
                      .WithMany("Decks")
                      .HasForeignKey("UserId")
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();
          });
#pragma warning restore 612, 618
    }
  }
}
