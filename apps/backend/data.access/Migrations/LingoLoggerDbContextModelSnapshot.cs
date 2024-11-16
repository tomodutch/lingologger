﻿// <auto-generated />
using System;
using LingoLogger.Data.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace data.access.Migrations
{
    [DbContext(typeof(LingoLoggerDbContext))]
    partial class LingoLoggerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LingoLogger.Data.Models.Goal", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamptz");

                    b.Property<DateTimeOffset?>("EndsAt")
                        .IsRequired()
                        .HasColumnType("timestamptz");

                    b.Property<int>("TargetTimeInSeconds")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Goals", (string)null);
                });

            modelBuilder.Entity("LingoLogger.Data.Models.Log", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                    b.Property<int>("AmountOfSeconds")
                        .HasMaxLength(86400)
                        .HasColumnType("integer");

                    b.Property<int?>("CharactersRead")
                        .HasColumnType("integer");

                    b.Property<double>("Coefficient")
                        .HasMaxLength(86400)
                        .HasColumnType("double precision");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamptz");

                    b.Property<string>("LogType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MediumId")
                        .HasColumnType("uuid");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("SourceEventId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("UserId", "CreatedAt");

                    b.HasIndex("MediumId");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId", "MediumId");

                    b.HasIndex("UserId", "SourceEventId");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.Medium", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamptz");

                    b.Property<long?>("GuildId")
                        .HasColumnType("bigint");

                    b.Property<string>("LogType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.HasKey("Id");

                    b.ToTable("Media", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("c732a6f7-ccc6-44ad-88aa-e1da57e16c4a"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Readable",
                            Name = "Book"
                        },
                        new
                        {
                            Id = new Guid("46906a9f-1062-45fc-b5cd-8b30fa97062a"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Readable",
                            Name = "Visual Novel"
                        },
                        new
                        {
                            Id = new Guid("8b51cd17-c2f0-4b4b-99ee-63c99924d107"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Audible",
                            Name = "Podcast"
                        },
                        new
                        {
                            Id = new Guid("e11ca2ad-9903-4926-a21e-6640fac79089"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Audible",
                            Name = "Audiobook"
                        },
                        new
                        {
                            Id = new Guid("676fb136-e379-4c6a-ad6a-b4aaad2e413e"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Watchable",
                            Name = "Anime"
                        },
                        new
                        {
                            Id = new Guid("21daae4a-3dfc-4f8b-b525-0a513376de1a"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Watchable",
                            Name = "Youtube"
                        },
                        new
                        {
                            Id = new Guid("bb2adabb-1271-468f-af03-b8457e3e6488"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Anki",
                            Name = "Anki"
                        },
                        new
                        {
                            Id = new Guid("4bce7a21-3b1d-4ffd-a7eb-ea50724bf629"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Other",
                            Name = "Other"
                        },
                        new
                        {
                            Id = new Guid("ca56bf69-0f46-44a5-a052-cd878ebdf757"),
                            CreatedAt = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            LogType = "Writing",
                            Name = "Other"
                        });
                });

            modelBuilder.Entity("LingoLogger.Data.Models.Milestone", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamptz");

                    b.Property<DateTimeOffset?>("ReachedAt")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("milestones", (string)null);
                });

            modelBuilder.Entity("LingoLogger.Data.Models.TogglIntegration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamptz");

                    b.Property<bool>("IsVerified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("WebhookSecret")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("toggleIntegrations", (string)null);
                });

            modelBuilder.Entity("LingoLogger.Data.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("DiscordId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.Goal", b =>
                {
                    b.HasOne("LingoLogger.Data.Models.User", "User")
                        .WithMany("Goals")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.Log", b =>
                {
                    b.HasOne("LingoLogger.Data.Models.Medium", "Medium")
                        .WithMany("Logs")
                        .HasForeignKey("MediumId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LingoLogger.Data.Models.User", "User")
                        .WithMany("Logs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Medium");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.Milestone", b =>
                {
                    b.HasOne("LingoLogger.Data.Models.User", "User")
                        .WithMany("Milestones")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.TogglIntegration", b =>
                {
                    b.HasOne("LingoLogger.Data.Models.User", "User")
                        .WithMany("TogglIntegrations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.Medium", b =>
                {
                    b.Navigation("Logs");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.User", b =>
                {
                    b.Navigation("Goals");

                    b.Navigation("Logs");

                    b.Navigation("Milestones");

                    b.Navigation("TogglIntegrations");
                });
#pragma warning restore 612, 618
        }
    }
}
