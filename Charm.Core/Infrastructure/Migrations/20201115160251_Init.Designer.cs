﻿// <auto-generated />
using System;
using Charm.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Charm.Core.Infrastructure.Migrations
{
    [DbContext(typeof(CharmDbContext))]
    [Migration("20201115160251_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Charm.Core.Infrastructure.Entities.Reminder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("Advance")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("Deadline")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Reminder");
                });

            modelBuilder.Entity("Charm.Core.Infrastructure.Entities.Task", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Gist")
                        .HasColumnType("text");

                    b.Property<Guid?>("ParentTaskId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ReminderId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ParentTaskId");

                    b.HasIndex("ReminderId")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Task");
                });

            modelBuilder.Entity("Charm.Core.Infrastructure.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DialogId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Charm.Core.Infrastructure.Entities.Task", b =>
                {
                    b.HasOne("Charm.Core.Infrastructure.Entities.Task", "ParentTask")
                        .WithMany()
                        .HasForeignKey("ParentTaskId");

                    b.HasOne("Charm.Core.Infrastructure.Entities.Reminder", "Reminder")
                        .WithOne("Task")
                        .HasForeignKey("Charm.Core.Infrastructure.Entities.Task", "ReminderId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Charm.Core.Infrastructure.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("ParentTask");

                    b.Navigation("Reminder");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Charm.Core.Infrastructure.Entities.Reminder", b =>
                {
                    b.Navigation("Task");
                });
#pragma warning restore 612, 618
        }
    }
}
