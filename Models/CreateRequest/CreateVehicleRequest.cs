namespace Minecraft.Models.CreateRequest
{
    public class CreateVehicleRequest
    {
        public int VehicleID { get; set; }
        public string VehicleName { get; set; }
        public string VehicleType { get; set; }
        public string ImageURL { get; set; }
        public int Price { get; set; }
    }
}
