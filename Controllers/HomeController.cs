using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minecraft.Data;
using Minecraft.Models;
using Minecraft.Models.AccountModels;
using Minecraft.Models.CreateRequest;
using Minecraft.Models.ViewModels;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Minecraft.Models.CreateRequest.CreatePurchaseRequest;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Minecraft.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        protected ResponseAPI _response;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _response = new();
            _configuration = configuration;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private string GenerateJwtToken(Player player)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, player.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, player.PlayerID.ToString())
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var data = new DatabaseViewModel
            {
                Players = _context.Players
                    .Include(p => p.GameMode)
                    .ToList(),
                Items = _context.Items.ToList(),
                Vehicles = _context.Vehicles.ToList(),
                GameModes = _context.GameModes.ToList(),
                Quests = _context.Quests.ToList(),
                Purchases = _context.Purchases
                    .Include(p => p.Player)
                    .Include(p => p.Item)
                    .ToList(),
                MonsterKills = _context.MonsterKills
                    .Include(mk => mk.Player)
                    .ToList(),
                PlayerQuests = _context.PlayerQuests
                    .Include(mk => mk.Player)
                    .Include(mk => mk.Quest)
                    .ToList(),
                Monsters = _context.Monsters.ToList()
            };

            return View(data);
        }

        [HttpPost("PostPlayer")]
        public async Task<IActionResult> AddPlayer([FromBody] CreatePlayerRequest req)
        {
            var player = new Player
            {
                Email = req.Email,
                Password = req.Password,
                CharacterName = req.CharacterName,
                ExperiencePoints = req.ExperiencePoints,
                ModeID = req.ModeID
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "✅ Player created successfully!", Data = player });
        }
        [HttpPost("PostItem")]
        public async Task<IActionResult> AddItem([FromBody] CreateItemRequest req)
        {
            var item = new Item
            {
                ItemName = req.ItemName,
                ItemType = req.ItemType,
                ImageURL = req.ImageURL,
                Price = req.Price
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "✅ Item created successfully!", Data = item });
        }
        [HttpPost("PostVehicle")]
        public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleRequest req)
        {
            var vehicle = new Vehicle
            {
                VehicleName = req.VehicleName,
                VehicleType = req.VehicleType,
                ImageURL = req.ImageURL,
                Price = req.Price
            };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "✅ Vehicle created successfully!", Data = vehicle });
        }

        [HttpPost("PostMonster")]
        public async Task<IActionResult> AddMonster([FromBody] CreateMonsterRequest req)
        {
            var monster = new Monster
            {
                MonsterName = req.MonsterName,
                Health = req.Health,
                AttackDamage = req.AttackDamage
            };
            _context.Monsters.Add(monster);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "✅ MonsterKill created successfully!", Data = monster });
        }

        [Authorize]
        [HttpGet("GetAllPlayer")]
        public async Task<IActionResult> GetAllPlayer()
        {
            try
            {
                var players = await (from p in _context.Players
                                     join gm in _context.GameModes on p.ModeID equals gm.ModeID
                                     select new PlayerDTO
                                     {
                                         PlayerID = p.PlayerID,
                                         Email = p.Email,
                                         Password = p.Password,
                                         CharacterName = p.CharacterName,
                                         GameModeName = gm.ModeName,
                                         ExperiencePoints = p.ExperiencePoints,
                                         WalletBalance = p.WalletBalance
                                     }).OrderByDescending(p => p.ExperiencePoints).ToListAsync();

                var response = new ResponsePlayerListModel<List<PlayerDTO>>()
                {
                    IsSuccess = true,
                    Notification = "Lấy dữ liệu thành công",
                    Data = players
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponsePlayerListModel<string>()
                {
                    IsSuccess = false,
                    Notification = "Lỗi",
                    Data = ex.Message
                };
                return BadRequest(response);
            }
        }

        [Authorize]
        [HttpGet("GetItemVehicleList")]
        public async Task<IActionResult> GetItemVehicleList()
        {
            try
            {
                var itemList = await (from i in _context.Items
                                      select new ItemVehicleDTO
                                      {
                                          ItemID = i.ItemID,
                                          ItemName = i.ItemName,
                                          VehicleID = null,
                                          VehicleName = null,
                                          ImageUrl = i.ImageURL,
                                          Type = i.ItemType,
                                          Price = i.Price
                                      }).ToListAsync();

                var vehicleList = await (from v in _context.Vehicles
                                         select new ItemVehicleDTO
                                         {
                                             ItemID = null,
                                             ItemName = null,
                                             VehicleID = v.VehicleID,
                                             VehicleName = v.VehicleName,
                                             ImageUrl = v.ImageURL,
                                             Type = v.VehicleType,
                                             Price = v.Price
                                         }).ToListAsync();

                // GHÉP LẠI THÀNH MỘT LIST
                var itemVehicleList = itemList.Concat(vehicleList).ToList();

                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công!";
                _response.data = itemVehicleList;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi!";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpPost("PostPurchase")]
        public async Task<IActionResult> PostPurchase([FromBody] PurchaseRequestDTO request)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (request.items == null || request.items.Count == 0)
            {
                return BadRequest(new { isSuccess = false, notification = "Giỏ hàng rỗng." });
            }

            // 2. LẤY PLAYER ID TỪ JSON CLIENT GỬI
            // (Lấy từ món hàng đầu tiên vì tất cả đều chung 1 người mua)
            int playerId = request.items[0].playerID;

            // 3. Tìm Player trong Database
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                return BadRequest(new { isSuccess = false, notification = "ID Người chơi không tồn tại trong hệ thống." });
            }

            // 4. Kiểm tra số dư ví
            if (player.WalletBalance < request.totalCost)
            {
                return BadRequest(new { isSuccess = false, notification = "Số dư không đủ." });
            }

            // 5. Thực hiện giao dịch (Transaction)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. Trừ tiền
                player.WalletBalance -= request.totalCost;
                _context.Entry(player).Property(x => x.WalletBalance).IsModified = true;

                // B. Lưu lịch sử mua hàng
                foreach (var itemDto in request.items)
                {
                    if (itemDto.quantity <= 0) continue;

                    var purchase = new Purchase
                    {
                        PlayerID = playerId, // Sử dụng ID client gửi lên
                        PurchaseDate = DateTime.Now,
                        Amount = itemDto.quantity,

                        // Xử lý ItemID: Nếu > 0 thì lấy, nếu bằng 0 hoặc null thì để null
                        ItemID = (itemDto.itemID.HasValue && itemDto.itemID.Value > 0) ? itemDto.itemID : null,

                        // Xử lý VehicleID
                        VehicleID = (itemDto.vehicleID.HasValue && itemDto.vehicleID.Value > 0) ? itemDto.vehicleID : null
                    };

                    _context.Purchases.Add(purchase);
                }

                // C. Lưu xuống DB
                await _context.SaveChangesAsync();

                // D. Commit transaction
                await transaction.CommitAsync();

                return Ok(new
                {
                    isSuccess = true,
                    notification = "Mua hàng thành công!",
                    newBalance = player.WalletBalance
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { isSuccess = false, notification = "Lỗi Server: " + ex.Message });
            }
        }


        [HttpPost("LoginRequest")]
        public async Task<IActionResult> LoginRequest([FromBody] LoginModel req)
        {
            if (req == null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
            {
                _response.isSuccess = false;
                _response.notification = "Thiếu Email hoặc Password!";
                return BadRequest(_response);
            }

            var player = await _context.Players
                        .FirstOrDefaultAsync(p => p.Email == req.Email && p.Password == req.Password);

            if (player == null)
            {
                _response.isSuccess = false;
                _response.notification = "Sai tài khoản hoặc mật khẩu!";
                return Unauthorized(_response);
            }
            else
            {
                _response.isSuccess = true;
                _response.notification = $"Xin chào {player.CharacterName}";
                _response.token = GenerateJwtToken(player);
                _response.loggedInName = player.CharacterName;
                _response.playerID = player.PlayerID;
                _response.walletBalance = player.WalletBalance;
                return Ok(_response);
            }
        }

        [HttpPost("RegisterRequest")]
        public async Task<IActionResult> RegisterRequest([FromBody] RegisterModel req)
        {
            if(req == null || string.IsNullOrEmpty(req.CharacterName) || string.IsNullOrEmpty(req.Email)
                || string.IsNullOrEmpty(req.Password))
            {
                _response.isSuccess = false;
                _response.notification = "Vui lòng nhập đầy đủ thông tin!";
                return BadRequest(_response);
            }
            var checkEmailPlayer = await _context.Players
                        .FirstOrDefaultAsync(p => p.Email == req.Email);
            if (checkEmailPlayer != null)
            {
                _response.isSuccess = false;
                _response.notification = "Email đã tồn tại, vui lòng thử lại!";
                checkEmailPlayer = null;
                return BadRequest(_response);
            }

            var checkCharNamePlayer = await _context.Players
                        .FirstOrDefaultAsync(p => p.CharacterName == req.CharacterName);
            if (checkCharNamePlayer != null)
            {
                _response.isSuccess = false;
                _response.notification = "Tên nhân vật đã tồn tại!";
                checkCharNamePlayer = null;
                return BadRequest(_response);
            }
            var newPlayer = new Player
            {
                CharacterName = req.CharacterName,
                Email = req.Email,
                Password = req.Password,
                ModeID = req.GameModeID,
                ExperiencePoints = 0,
                WalletBalance = 0
            };
            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            _response.isSuccess = true;
            _response.notification = "Đăng ký thành công!";
            _response.token = GenerateJwtToken(newPlayer);
            _response.loggedInName = newPlayer.CharacterName;
            _response.playerID = newPlayer.PlayerID;
            _response.walletBalance = newPlayer.WalletBalance;
            return Ok(_response);
        }

        [Authorize]
        [HttpGet("GetAllItem")]
        public async Task<IActionResult> GetAllItem()
        {
            try
            {
                var roles = await _context.Items.ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = roles;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpGet("GetAllVehicle")]
        public async Task<IActionResult> GetAllVehicle()
        {
            try
            {
                var vehicles = await _context.Vehicles.ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = vehicles;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpGet("GetAllLevelResult")]
        public async Task<IActionResult> GetAllLevelResult()
        {
            try
            {
                var levelResults = await _context.Vehicles.ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = levelResults;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }


        [HttpGet("GetAllGameMode")]
        public async Task<IActionResult> GetAllGameMode()
        {
            try
            {
                var gameLevel = await _context.GameModes.ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = gameLevel;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpGet("GetAllQuest")]
        public async Task<IActionResult> GetAllQuest()
        {
            try
            {
                var quest = await _context.Quests.ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = quest;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpGet("GetAllPurchase")]
        public async Task<IActionResult> GetAllPurchase()
        {
            try
            {
                var purchase = await _context.Purchases
                    .Include(p => p.Player)
                    .Include(p => p.Item)
                    .Include(p => p.Vehicle)
                    .ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = purchase;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpGet("GetAllPlayerQuest")]
        public async Task<IActionResult> GetAllPlayerQuest()
        {
            try
            {
                var playerQuest = await _context.PlayerQuests
                    .Include(pq => pq.Player)
                    .Include(pq => pq.Quest)
                    .ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = playerQuest;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpGet("GetAllMonsterKill")]
        public async Task<IActionResult> GetAllMonsterKill()
        {
            try
            {
                var monsterKill = await _context.MonsterKills
                    .Include(mk => mk.Player)
                    .Include(mk => mk.Monster)
                    .ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = monsterKill;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }
        [HttpGet("GetAllMonster")]
        public async Task<IActionResult> GetAllMonster()
        {
            try
            {
                var monster = await _context.Monsters.ToListAsync();
                _response.isSuccess = true;
                _response.notification = "Lấy dữ liệu thành công";
                _response.data = monster;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdatePlayer(int id, [FromBody] Player updatePlayer)
        {
            try
            {
                var player = await _context.Players.FirstOrDefaultAsync(x => x.PlayerID == id);

                if (player == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy User";
                    _response.data = null;
                    return NotFound(_response);
                }

                // Cập nhật dữ liệu
                player.Email = updatePlayer.Email;
                player.Password = updatePlayer.Password;
                player.CharacterName = updatePlayer.CharacterName;
                player.ExperiencePoints = updatePlayer.ExperiencePoints;
                player.ModeID = updatePlayer.ModeID;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                _response.isSuccess = true;
                _response.notification = "Cập nhật Player thành công";
                _response.data = player;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpPut("UpdateItem/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item updateItem)
        {
            try
            {
                var item = await _context.Items.FirstOrDefaultAsync(x => x.ItemID == id);
                if (item == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy Item";
                    _response.data = null;
                    return NotFound(_response);
                }
                // Cập nhật dữ liệu
                item.ItemName = updateItem.ItemName;
                item.ItemType = updateItem.ItemType;
                item.ImageURL = updateItem.ImageURL;
                item.Price = updateItem.Price;
                // Lưu thay đổi
                await _context.SaveChangesAsync();
                _response.isSuccess = true;
                _response.notification = "Cập nhật Item thành công";
                _response.data = item;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpPut("UpdateVehicle/{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] Vehicle updateVehicle)
        {
            try
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.VehicleID == id);
                if (vehicle == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy Vehicle";
                    _response.data = null;
                    return NotFound(_response);
                }
                // Cập nhật dữ liệu
                vehicle.VehicleName = updateVehicle.VehicleName;
                vehicle.VehicleType = updateVehicle.VehicleType;
                vehicle.ImageURL = updateVehicle.ImageURL;
                vehicle.Price = updateVehicle.Price;
                // Lưu thay đổi
                await _context.SaveChangesAsync();
                _response.isSuccess = true;
                _response.notification = "Cập nhật Vehicle thành công";
                _response.data = vehicle;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpPut("UpdateMonster/{id}")]
        public async Task<IActionResult> UpdateMonster(int id, [FromBody] Monster updateMonster)
        {
            try
            {
                var monster = await _context.Monsters.FirstOrDefaultAsync(x => x.MonsterID == id);
                if (monster == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy Monster";
                    _response.data = null;
                    return NotFound(_response);
                }
                // Cập nhật dữ liệu
                monster.MonsterName = updateMonster.MonsterName;
                monster.Health = updateMonster.Health;
                monster.AttackDamage = updateMonster.AttackDamage;
                // Lưu thay đổi
                await _context.SaveChangesAsync();
                _response.isSuccess = true;
                _response.notification = "Cập nhật Monster thành công";
                _response.data = monster;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpDelete("DeletePlayer/{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            try
            {
                var player = await _context.Players.FindAsync(id);
                if (player == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy Player";
                    _response.data = null;
                    return NotFound(_response);
                }
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
                _response.isSuccess = true;
                _response.notification = "Xóa Player thành công";
                _response.data = player;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpDelete("DeleteItem/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy Item";
                    _response.data = null;
                    return NotFound(_response);
                }
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                _response.isSuccess = true;
                _response.notification = "Xóa Item thành công";
                _response.data = item;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpDelete("DeleteVehicle/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy Vehicle";
                    _response.data = null;
                    return NotFound(_response);
                }
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                _response.isSuccess = true;
                _response.notification = "Xóa Vehicle thành công";
                _response.data = vehicle;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [Authorize]
        [HttpDelete("DeleteMonster/{id}")]
        public async Task<IActionResult> DeleteMonster(int id)
        {
            try
            {
                var monster = await _context.Monsters.FindAsync(id);
                if (monster == null)
                {
                    _response.isSuccess = false;
                    _response.notification = "Không tìm thấy Monster";
                    _response.data = null;
                    return NotFound(_response);
                }
                _context.Monsters.Remove(monster);
                await _context.SaveChangesAsync();
                _response.isSuccess = true;
                _response.notification = "Xóa Monster thành công";
                _response.data = monster;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.notification = "Lỗi";
                _response.data = ex.Message;
                return BadRequest(_response);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
