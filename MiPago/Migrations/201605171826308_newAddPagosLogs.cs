namespace MiPago.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newAddPagosLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FechaCreacion = c.DateTimeOffset(nullable: false, precision: 7),
                        Modulo = c.String(nullable: false),
                        Detalle = c.String(nullable: false),
                        PagoId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Pagos", t => t.PagoId)
                .Index(t => t.PagoId);
            
            CreateTable(
                "dbo.Pagos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        TbkRespuesta = c.String(unicode: false, storeType: "text"),
                        TokenAcceso = c.String(nullable: false),
                        AppEmail = c.String(nullable: false),
                        AppOrdenId = c.String(nullable: false),
                        Monto = c.Int(nullable: false),
                        MiPagoOrdenId = c.String(),
                        FechaCreacion = c.DateTimeOffset(nullable: false, precision: 7),
                        FechaPago = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "PagoId", "dbo.Pagos");
            DropForeignKey("dbo.Pagos", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Pagos", new[] { "UserId" });
            DropIndex("dbo.Logs", new[] { "PagoId" });
            DropTable("dbo.Pagos");
            DropTable("dbo.Logs");
        }
    }
}
