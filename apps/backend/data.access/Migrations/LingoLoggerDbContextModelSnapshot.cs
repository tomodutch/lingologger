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

                    b.Property<string>("Notes")
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
                    b.HasOne("LingoLogger.Data.Models.User", "User")
                        .WithMany("Logs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
