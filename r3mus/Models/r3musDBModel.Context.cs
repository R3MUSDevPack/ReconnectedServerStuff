﻿

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace r3mus.Models
{

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;


public partial class r3mus_DBEntities : DbContext
{
    public r3mus_DBEntities()
        : base("name=r3mus_DBEntities")
    {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        throw new UnintentionalCodeFirstException();
    }


    public virtual DbSet<CRONJob> CRONJobs { get; set; }

    public virtual DbSet<OnlineUser> OnlineUsers { get; set; }

    public virtual DbSet<LogMessage> LogMessages { get; set; }

    public virtual DbSet<DeclaredToon> DeclaredToons { get; set; }

    public virtual DbSet<Fleet> Fleets { get; set; }

    public virtual DbSet<FleetComposition> FleetCompositions { get; set; }

    public virtual DbSet<Applicant> Applicants { get; set; }

    public virtual DbSet<RecruitmentMailee> RecruitmentMailees { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

}

}

