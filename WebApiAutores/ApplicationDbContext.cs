﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AutoresLibros>().HasKey(al=> new {al.AutorId, al.LibroId });

        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; } 
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutoresLibros> AutoresLibros { get; set; }
    }
}
