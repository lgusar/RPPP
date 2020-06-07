using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KoronavirusMvc.Models
{
    public partial class RPPP09Context : DbContext
    {
        public RPPP09Context(DbContextOptions<RPPP09Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Drzava> Drzava { get; set; }
        public virtual DbSet<Institucija> Institucija { get; set; }
        public virtual DbSet<Kontakt> Kontakt { get; set; }
        public virtual DbSet<Lokacija> Lokacija { get; set; }
        public virtual DbSet<Mjera> Mjera { get; set; }
        public virtual DbSet<Oprema> Oprema { get; set; }
        public virtual DbSet<Organizacija> Organizacija { get; set; }
        public virtual DbSet<Osoba> Osoba { get; set; }
        public virtual DbSet<OsobaPregled> OsobaPregled { get; set; }
        public virtual DbSet<Pregled> Pregled { get; set; }
        public virtual DbSet<PregledSimptom> PregledSimptom { get; set; }
        public virtual DbSet<PregledTerapija> PregledTerapija { get; set; }
        public virtual DbSet<Preporuka> Preporuka { get; set; }
        public virtual DbSet<Putovanje> Putovanje { get; set; }
        public virtual DbSet<PutovanjeLokacija> PutovanjeLokacija { get; set; }
        public virtual DbSet<Sastanak> Sastanak { get; set; }
        public virtual DbSet<Simptom> Simptom { get; set; }
        public virtual DbSet<Stanje> Stanje { get; set; }
        public virtual DbSet<Statistika> Statistika { get; set; }
        public virtual DbSet<Stozer> Stozer { get; set; }
        public virtual DbSet<StozerOsoba> StozerOsoba { get; set; }
        public virtual DbSet<Terapija> Terapija { get; set; }
        public virtual DbSet<ZarazenaOsoba> ZarazenaOsoba { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Drzava>(entity =>
            {
                entity.HasKey(e => e.SifraDrzave);

                entity.ToTable("DRZAVA");

                entity.Property(e => e.SifraDrzave)
                    .HasColumnName("sifra_drzave")
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.ImeDrzave)
                    .IsRequired()
                    .HasColumnName("ime_drzave")
                    .HasMaxLength(80)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Institucija>(entity =>
            {
                entity.HasKey(e => e.SifraInstitucije);

                entity.ToTable("INSTITUCIJA");

                entity.Property(e => e.SifraInstitucije)
                    .HasColumnName("sifra_institucije")
                    .ValueGeneratedNever();

                entity.Property(e => e.Kontakt)
                    .IsRequired()
                    .HasColumnName("kontakt")
                    .HasMaxLength(20)
                    .IsFixedLength();

                entity.Property(e => e.NazivInstitucije)
                    .IsRequired()
                    .HasColumnName("naziv_institucije")
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.Property(e => e.RadnoVrijeme).HasColumnName("radno_vrijeme");

                entity.Property(e => e.SifraOrganizacije).HasColumnName("sifra_organizacije");

                entity.HasOne(d => d.SifraOrganizacijeNavigation)
                    .WithMany(p => p.Institucija)
                    .HasForeignKey(d => d.SifraOrganizacije)
                    .HasConstraintName("FK_INSTITUCIJA_ORGANIZACIJA");
            });

            modelBuilder.Entity<Kontakt>(entity =>
            {
                entity.HasKey(e => new { e.IdOsoba, e.IdKontakt })
                    .HasName("PK__KONTAKT__0CAC1D702E696187");

                entity.ToTable("KONTAKT");

                entity.Property(e => e.IdOsoba)
                    .HasColumnName("id_osoba")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.Property(e => e.IdKontakt)
                    .HasColumnName("id_kontakt")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.HasOne(d => d.IdKontaktNavigation)
                    .WithMany(p => p.KontaktIdKontaktNavigation)
                    .HasForeignKey(d => d.IdKontakt)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KONTAKT__id_kont__3A81B327");

                entity.HasOne(d => d.IdOsobaNavigation)
                    .WithMany(p => p.KontaktIdOsobaNavigation)
                    .HasForeignKey(d => d.IdOsoba)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KONTAKT__id_osob__398D8EEE");
            });

            modelBuilder.Entity<Lokacija>(entity =>
            {
                entity.HasKey(e => e.SifraGrada);

                entity.ToTable("LOKACIJA");

                entity.Property(e => e.SifraGrada)
                    .HasColumnName("sifra_grada")
                    .ValueGeneratedNever();

                entity.Property(e => e.ImeGrada)
                    .IsRequired()
                    .HasColumnName("ime_grada")
                    .HasMaxLength(25)
                    .IsFixedLength();

                entity.Property(e => e.SifraDrzave)
                    .IsRequired()
                    .HasColumnName("sifra_drzave")
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.HasOne(d => d.SifraDrzaveNavigation)
                    .WithMany(p => p.Lokacija)
                    .HasForeignKey(d => d.SifraDrzave)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LOKACIJA_DRZAVA");
            });

            modelBuilder.Entity<Mjera>(entity =>
            {
                entity.HasKey(e => e.SifraMjere);

                entity.ToTable("MJERA");

                entity.Property(e => e.SifraMjere)
                    .HasColumnName("sifra_mjere")
                    .ValueGeneratedNever();

                entity.Property(e => e.Datum)
                    .HasColumnName("datum")
                    .HasColumnType("date");

                entity.Property(e => e.Opis)
                    .IsRequired()
                    .HasColumnName("opis")
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.Property(e => e.SifraPrethodneMjere).HasColumnName("sifra_prethodne_mjere");

                entity.Property(e => e.SifraSastanka).HasColumnName("sifra_sastanka");

                entity.Property(e => e.VrijediDo)
                    .HasColumnName("vrijedi_do")
                    .HasColumnType("date");

                entity.HasOne(d => d.SifraPrethodneMjereNavigation)
                    .WithMany(p => p.InverseSifraPrethodneMjereNavigation)
                    .HasForeignKey(d => d.SifraPrethodneMjere)
                    .HasConstraintName("FK_MJERA_MJERA");

                entity.HasOne(d => d.SifraSastankaNavigation)
                    .WithMany(p => p.Mjera)
                    .HasForeignKey(d => d.SifraSastanka)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MJERA_SASTANAK");
            });

            modelBuilder.Entity<Oprema>(entity =>
            {
                entity.HasKey(e => e.SifraOpreme);

                entity.ToTable("OPREMA");

                entity.Property(e => e.SifraOpreme)
                    .HasColumnName("sifra_opreme")
                    .ValueGeneratedNever();

                entity.Property(e => e.KolicinaOpreme).HasColumnName("kolicina_opreme");

                entity.Property(e => e.NazivOpreme)
                    .IsRequired()
                    .HasColumnName("naziv_opreme")
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.Property(e => e.SifraInstitucije).HasColumnName("sifra_institucije");

                entity.HasOne(d => d.SifraInstitucijeNavigation)
                    .WithMany(p => p.Oprema)
                    .HasForeignKey(d => d.SifraInstitucije)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OPREMA_INSTITUCIJA");
            });

            modelBuilder.Entity<Organizacija>(entity =>
            {
                entity.HasKey(e => e.SifraOrganizacije);

                entity.ToTable("ORGANIZACIJA");

                entity.Property(e => e.SifraOrganizacije)
                    .HasColumnName("sifra_organizacije")
                    .ValueGeneratedNever();

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasColumnName("naziv")
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.Property(e => e.Url)
                    .HasColumnName("URL")
                    .HasMaxLength(255)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Osoba>(entity =>
            {
                entity.HasKey(e => e.IdentifikacijskiBroj);

                entity.ToTable("OSOBA");

                entity.Property(e => e.IdentifikacijskiBroj)
                    .HasColumnName("identifikacijski_broj")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.Property(e => e.Adresa)
                    .IsRequired()
                    .HasColumnName("adresa")
                    .HasMaxLength(100)
                    .IsFixedLength();

                entity.Property(e => e.DatRod)
                    .HasColumnName("dat_rod")
                    .HasColumnType("date");

                entity.Property(e => e.Ime)
                    .IsRequired()
                    .HasColumnName("ime")
                    .HasMaxLength(20)
                    .IsFixedLength();

                entity.Property(e => e.Prezime)
                    .IsRequired()
                    .HasColumnName("prezime")
                    .HasMaxLength(40)
                    .IsFixedLength();

                entity.Property(e => e.Zanimanje)
                    .HasColumnName("zanimanje")
                    .HasMaxLength(50)
                    .IsFixedLength();
            });

            modelBuilder.Entity<OsobaPregled>(entity =>
            {
                entity.HasKey(e => e.SifraPregleda);

                entity.ToTable("OSOBA_PREGLED");

                entity.Property(e => e.IdentifikacijskiBroj)
                    .IsRequired()
                    .HasColumnName("identifikacijski_broj")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.Property(e => e.SifraPregleda).HasColumnName("sifra_pregleda");

                entity.HasOne(d => d.IdentifikacijskiBrojNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.IdentifikacijskiBroj)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK__OBAVLJA__identif__2A4B4B5E");

                entity.HasOne(d => d.SifraPregledaNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.SifraPregleda)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK__OBAVLJA__sifra_p__2B3F6F97");
            });

            modelBuilder.Entity<Pregled>(entity =>
            {
                entity.HasKey(e => e.SifraPregleda)
                    .HasName("PK__PREGLED__65C104B2AB126BC4");

                entity.ToTable("PREGLED");

                entity.Property(e => e.SifraPregleda)
                    .HasColumnName("sifra_pregleda")
                    .ValueGeneratedNever();

                entity.Property(e => e.Anamneza)
                    .HasColumnName("anamneza")
                    .HasMaxLength(255);

                entity.Property(e => e.Datum)
                    .HasColumnName("datum")
                    .HasColumnType("date");

                entity.Property(e => e.Dijagnoza)
                    .HasColumnName("dijagnoza")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<PregledSimptom>(entity =>
            {
                entity.HasKey(e => new { e.SifraSimptoma, e.SifraPregleda })
                    .HasName("PK__PREGLED___32D4AF6A3F5439A2");

                entity.ToTable("PREGLED_SIMPTOM");

                entity.Property(e => e.SifraSimptoma).HasColumnName("sifra_simptoma");

                entity.Property(e => e.SifraPregleda).HasColumnName("sifra_pregleda");

                entity.HasOne(d => d.SifraPregledaNavigation)
                    .WithMany(p => p.PregledSimptom)
                    .HasForeignKey(d => d.SifraPregleda)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK__PREGLED_S__sifra__0E6E26BF");

                entity.HasOne(d => d.SifraSimptomaNavigation)
                    .WithMany(p => p.PregledSimptom)
                    .HasForeignKey(d => d.SifraSimptoma)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK_PREGLED_SIMPTOM_SIMPTOM");
            });

            modelBuilder.Entity<PregledTerapija>(entity =>
            {
                entity.HasKey(e => new { e.SifraPregleda, e.SifraTerapije })
                    .HasName("PK__PREGLED___6685DC66C3706E02");

                entity.ToTable("PREGLED_TERAPIJA");

                entity.Property(e => e.SifraPregleda).HasColumnName("sifra_pregleda");

                entity.Property(e => e.SifraTerapije).HasColumnName("sifra_terapije");

                entity.HasOne(d => d.SifraPregledaNavigation)
                    .WithMany(p => p.PregledTerapija)
                    .HasForeignKey(d => d.SifraPregleda)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK__PROPISUJE__sifra__276EDEB3");

                entity.HasOne(d => d.SifraTerapijeNavigation)
                    .WithMany(p => p.PregledTerapija)
                    .HasForeignKey(d => d.SifraTerapije)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK__PROPISUJE__sifra__286302EC");
            });

            modelBuilder.Entity<Preporuka>(entity =>
            {
                entity.HasKey(e => e.SifraPreporuke);

                entity.ToTable("PREPORUKA");

                entity.Property(e => e.SifraPreporuke)
                    .HasColumnName("sifra_preporuke")
                    .ValueGeneratedNever();

                entity.Property(e => e.Opis)
                    .IsRequired()
                    .HasColumnName("opis")
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.Property(e => e.SifraOrganizacije).HasColumnName("sifra_organizacije");

                entity.Property(e => e.SifraPrethodnePreporuke).HasColumnName("sifra_prethodne_preporuke");

                entity.Property(e => e.SifraStozera).HasColumnName("sifra_stozera");

                entity.Property(e => e.VrijemeObjave)
                    .HasColumnName("vrijeme_objave")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.SifraOrganizacijeNavigation)
                    .WithMany(p => p.Preporuka)
                    .HasForeignKey(d => d.SifraOrganizacije)
                    .HasConstraintName("FK_PREPORUKA_ORGANIZACIJA");

                entity.HasOne(d => d.SifraPrethodnePreporukeNavigation)
                    .WithMany(p => p.InverseSifraPrethodnePreporukeNavigation)
                    .HasForeignKey(d => d.SifraPrethodnePreporuke)
                    .HasConstraintName("FK_PREPORUKA_PREPORUKA");

                entity.HasOne(d => d.SifraStozeraNavigation)
                    .WithMany(p => p.Preporuka)
                    .HasForeignKey(d => d.SifraStozera)
                    .HasConstraintName("FK_PREPORUKA_STOZER");
            });

            modelBuilder.Entity<Putovanje>(entity =>
            {
                entity.HasKey(e => e.SifraPutovanja);

                entity.ToTable("PUTOVANJE");

                entity.HasIndex(e => e.IdentifikacijskiBroj)
                    .HasName("IX_PUTOVANJE")
                    .IsUnique();

                entity.Property(e => e.SifraPutovanja)
                    .HasColumnName("sifra_putovanja")
                    .ValueGeneratedNever();

                entity.Property(e => e.DatumPolaska)
                    .HasColumnName("datum_polaska")
                    .HasColumnType("date");

                entity.Property(e => e.DatumVracanja)
                    .HasColumnName("datum_vracanja")
                    .HasColumnType("date");

                entity.Property(e => e.IdentifikacijskiBroj)
                    .IsRequired()
                    .HasColumnName("identifikacijski_broj")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.HasOne(d => d.IdentifikacijskiBrojNavigation)
                    .WithOne(p => p.Putovanje)
                    .HasForeignKey<Putovanje>(d => d.IdentifikacijskiBroj)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PUTOVANJE_OSOBA");
            });

            modelBuilder.Entity<PutovanjeLokacija>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("PUTOVANJE_LOKACIJA");

                entity.HasIndex(e => e.SifraGrada)
                    .HasName("IX_PUTOVANJE_LOKACIJA");

                entity.HasIndex(e => e.SifraPutovanja)
                    .HasName("IX_PUTOVANJE_LOKACIJA_1");

                entity.Property(e => e.SifraGrada).HasColumnName("sifra_grada");

                entity.Property(e => e.SifraPutovanja).HasColumnName("sifra_putovanja");

                entity.HasOne(d => d.SifraGradaNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.SifraGrada)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SE_ODRZALO_NA_LOKACIJA");

                entity.HasOne(d => d.SifraPutovanjaNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.SifraPutovanja)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SE_ODRZALO_NA_PUTOVANJE");
            });

            modelBuilder.Entity<Sastanak>(entity =>
            {
                entity.HasKey(e => e.SifraSastanka);

                entity.ToTable("SASTANAK");

                entity.Property(e => e.SifraSastanka)
                    .HasColumnName("sifra_sastanka")
                    .ValueGeneratedNever();

                entity.Property(e => e.Datum)
                    .HasColumnName("datum")
                    .HasColumnType("date");

                entity.Property(e => e.SifraStozera).HasColumnName("sifra_stozera");

                entity.HasOne(d => d.SifraStozeraNavigation)
                    .WithMany(p => p.Sastanak)
                    .HasForeignKey(d => d.SifraStozera)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SASTANAK_SASTANAK");
            });

            modelBuilder.Entity<Simptom>(entity =>
            {
                entity.HasKey(e => e.SifraSimptoma);

                entity.ToTable("SIMPTOM");

                entity.Property(e => e.SifraSimptoma)
                    .HasColumnName("sifra_simptoma")
                    .ValueGeneratedNever();

                entity.Property(e => e.Opis)
                    .HasColumnName("opis")
                    .HasMaxLength(255)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Stanje>(entity =>
            {
                entity.HasKey(e => e.SifraStanja);

                entity.ToTable("STANJE");

                entity.Property(e => e.SifraStanja)
                    .HasColumnName("sifra_stanja")
                    .ValueGeneratedNever();

                entity.Property(e => e.NazivStanja)
                    .IsRequired()
                    .HasColumnName("naziv_stanja")
                    .HasMaxLength(255)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Statistika>(entity =>
            {
                entity.HasKey(e => e.SifraObjave);

                entity.ToTable("STATISTIKA");

                entity.Property(e => e.SifraObjave)
                    .HasColumnName("sifra_objave")
                    .ValueGeneratedNever();

                entity.Property(e => e.BrojAktivnih).HasColumnName("broj_aktivnih");

                entity.Property(e => e.BrojIzlijecenih).HasColumnName("broj_izlijecenih");

                entity.Property(e => e.BrojSlucajeva).HasColumnName("broj_slucajeva");

                entity.Property(e => e.BrojUmrlih).HasColumnName("broj_umrlih");

                entity.Property(e => e.Datum)
                    .HasColumnName("datum")
                    .HasColumnType("date");

                entity.Property(e => e.SifraGrada).HasColumnName("sifra_grada");

                entity.Property(e => e.SifraOrganizacije).HasColumnName("sifra_organizacije");

                entity.HasOne(d => d.SifraGradaNavigation)
                    .WithMany(p => p.Statistika)
                    .HasForeignKey(d => d.SifraGrada)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_STATISTIKA_LOKACIJA");

                entity.HasOne(d => d.SifraOrganizacijeNavigation)
                    .WithMany(p => p.Statistika)
                    .HasForeignKey(d => d.SifraOrganizacije)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_STATISTIKA_ORGANIZACIJA");
            });

            modelBuilder.Entity<Stozer>(entity =>
            {
                entity.HasKey(e => e.SifraStozera);

                entity.ToTable("STOZER");

                entity.Property(e => e.SifraStozera)
                    .HasColumnName("sifra_stozera")
                    .ValueGeneratedNever();

                entity.Property(e => e.IdPredsjednika)
                    .IsRequired()
                    .HasColumnName("id_predsjednika")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasColumnName("naziv")
                    .HasMaxLength(255)
                    .IsFixedLength();

                entity.HasOne(d => d.IdPredsjednikaNavigation)
                    .WithMany(p => p.Stozer)
                    .HasForeignKey(d => d.IdPredsjednika)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_STOZER_OSOBA");
            });

            modelBuilder.Entity<StozerOsoba>(entity =>
            {
                entity.HasKey(e => e.IdentifikacijskiBroj)
                    .HasName("PK_UCLANJEN_U_STOZERU");

                entity.ToTable("STOZER_OSOBA");

                entity.Property(e => e.IdentifikacijskiBroj)
                    .HasColumnName("identifikacijski_broj")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.Property(e => e.SifraStozera).HasColumnName("sifra_stozera");

                entity.HasOne(d => d.IdentifikacijskiBrojNavigation)
                    .WithOne(p => p.StozerOsoba)
                    .HasForeignKey<StozerOsoba>(d => d.IdentifikacijskiBroj)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UCLANJEN_U_STOZERU_OSOBA");

                entity.HasOne(d => d.SifraStozeraNavigation)
                    .WithMany(p => p.StozerOsoba)
                    .HasForeignKey(d => d.SifraStozera)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UCLANJEN_U_STOZERU_UCLANJEN_U_STOZERU");
            });

            modelBuilder.Entity<Terapija>(entity =>
            {
                entity.HasKey(e => e.SifraTerapije)
                    .HasName("PK__TERAPIJA__344D8D41A1A81F3E");

                entity.ToTable("TERAPIJA");

                entity.Property(e => e.SifraTerapije)
                    .HasColumnName("sifra_terapije")
                    .ValueGeneratedNever();

                entity.Property(e => e.OpisTerapije)
                    .IsRequired()
                    .HasColumnName("opis_terapije")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<ZarazenaOsoba>(entity =>
            {
                entity.HasKey(e => e.IdentifikacijskiBroj);

                entity.ToTable("ZARAZENA_OSOBA");

                entity.Property(e => e.IdentifikacijskiBroj)
                    .HasColumnName("identifikacijski_broj")
                    .HasMaxLength(15)
                    .IsFixedLength();

                entity.Property(e => e.DatZaraze)
                    .HasColumnName("dat_zaraze")
                    .HasColumnType("date");

                entity.Property(e => e.SifraStanja).HasColumnName("sifra_stanja");

                entity.HasOne(d => d.IdentifikacijskiBrojNavigation)
                    .WithOne(p => p.ZarazenaOsoba)
                    .HasForeignKey<ZarazenaOsoba>(d => d.IdentifikacijskiBroj)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZARAZENA_OSOBA_OSOBA");

                entity.HasOne(d => d.SifraStanjaNavigation)
                    .WithMany(p => p.ZarazenaOsoba)
                    .HasForeignKey(d => d.SifraStanja)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZARAZENA_OSOBA_STANJE");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
