﻿// <auto-generated />
using System;
using FileProtectorUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileProtectorUI.Migrations
{
    [DbContext(typeof(FileProtectorContext))]
    partial class FilterProtectorContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity("FileProtectorUI.HistoryEntry", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Allowed");

                    b.Property<string>("Path");

                    b.Property<string>("ProcessId");

                    b.Property<string>("ProcessName");

                    b.Property<DateTime>("TimeAccessed");

                    b.HasKey("Id");

                    b.ToTable("HistoryEntries");
                });
#pragma warning restore 612, 618
        }
    }
}