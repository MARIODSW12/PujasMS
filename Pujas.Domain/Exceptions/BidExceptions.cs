

namespace Pujas.Domain.Exceptions
{
    public class InvalidPriceException : Exception
    {
        public InvalidPriceException() : base("El precio debe ser mayor a 0") { }
    }
    public class InvalidIdException : Exception
    {
        public InvalidIdException() : base("El id de la puja es invalido") { }
    }

    public class InvalidUserIdException : Exception
    {
        public InvalidUserIdException() : base("El id del usuario es invalido") { }
    }
    public class InvalidAuctionIdException : Exception
    {
        public InvalidAuctionIdException() : base("El id de la subasta es invalido") { }
    }

    public class InvalidDateException : Exception
    {
        public InvalidDateException() : base("La fecha de la puja no puede mayor a este momento") { }
    }

    public class MongoDBConnectionException : Exception 
    {
        public MongoDBConnectionException() : base("Error al conectar con la base de datos de mongo") { }
    }

    public class MongoDBUnnexpectedException : Exception
    {
        public MongoDBUnnexpectedException() : base("Error inesperado con la base de datos de mongo") { }
    }
}
