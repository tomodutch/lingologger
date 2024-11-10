﻿// <auto-generated />
using System;
using LingoLogger.Data.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace data.access.Migrations
{
    [DbContext(typeof(LingoLoggerDbContext))]
    [Migration("20241110141332_AddVerified")]
    partial class AddVerified
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<double>("Coefficient")
                        .HasMaxLength(86400)
                        .HasColumnType("double precision");

                    b.Property<DateTimeOffset?>("DeletedAt")
                        .HasColumnType("timestamptz");

                    b.Property<string>("LogType")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("character varying(13)");

                    b.Property<string>("Medium")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

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

                    b.HasIndex("UserId");

                    b.HasIndex("UserId", "SourceEventId");

                    b.ToTable("Logs");

                    b.HasDiscriminator<string>("LogType").HasValue("Log");

                    b.UseTphMappingStrategy();
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

                    b.Property<DateTimeOffset>("DeletedAt")
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

            modelBuilder.Entity("LingoLogger.Data.Models.AudibleLog", b =>
                {
                    b.HasBaseType("LingoLogger.Data.Models.Log");

                    b.HasDiscriminator().HasValue("Audible");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.EpisodicLog", b =>
                {
                    b.HasBaseType("LingoLogger.Data.Models.Log");

                    b.Property<int>("EpisodeLengthInSeconds")
                        .HasColumnType("integer");

                    b.Property<int>("Episodes")
                        .HasColumnType("integer");

                    b.HasDiscriminator().HasValue("Episodic");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.ReadableLog", b =>
                {
                    b.HasBaseType("LingoLogger.Data.Models.Log");

                    b.Property<int?>("CharactersRead")
                        .HasColumnType("integer");

                    b.HasDiscriminator().HasValue("Readable");
                });

            modelBuilder.Entity("LingoLogger.Data.Models.WatchableLog", b =>
                {
                    b.HasBaseType("LingoLogger.Data.Models.Log");

                    b.HasDiscriminator().HasValue("Watchable");
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
                    b.HasOne("LingoLogger.Data.Models.User", "User")
                        .WithMany("Logs")
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

            modelBuilder.Entity("LingoLogger.Data.Models.User", b =>
                {
                    b.Navigation("Goals");

                    b.Navigation("Logs");

                    b.Navigation("TogglIntegrations");
                });
#pragma warning restore 612, 618
        }
    }
}
