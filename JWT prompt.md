**JWT prompt**

**appsettings.json**



"JwtSettings": {

&#x20;   "Secret": "MyApplication\_Secret\_Key\_2026\_Keep\_It\_Safe!!",

&#x20;   "Issuer": "MyApplication",

&#x20;   "Audience": "MyApplicationUsers",

&#x20;   "ExpiryMinutes": 5

},





**program.cs**



builder.Services.AddAuthentication(options =>

{

&#x20;   options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;//authenticate by looking

&#x20;                                                                              //for a JWT token in the request header

&#x20;   options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//Challenge the client to provide a token

&#x20;                                                                           //if they try to access a protected resource without one

})

.AddJwtBearer(options =>

{

&#x20;   options.TokenValidationParameters = new TokenValidationParameters

&#x20;   {

&#x20;       ValidateIssuerSigningKey = true,

&#x20;       IssuerSigningKey = new SymmetricSecurityKey(key),

&#x20;       ValidateIssuer = true,

&#x20;       ValidateAudience = true,

&#x20;       ValidIssuer = jwtSettings\["Issuer"],

&#x20;       ValidAudience = jwtSettings\["Audience"],

&#x20;       ClockSkew = TimeSpan.Zero

&#x20;   };

});



**authcontroller.cs**



using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

using Microsoft.IdentityModel.Tokens;

using MyApp.API.Models;



namespace MyApp.API.Controllers

{

&#x20;   \[Route("api/\[controller]")]

&#x20;   \[ApiController]

&#x20;   public class AuthController : ControllerBase

&#x20;   {

&#x20;       private readonly IConfiguration \_configuration;

&#x20;       public AuthController(IConfiguration configuration)

&#x20;       {

&#x20;           \_configuration=configuration;

&#x20;       }

&#x20;       \[HttpPost("login")]



&#x20;       //Task<IActionResult> if you want to make it asynchronous,

&#x20;       //but for simplicity, we will keep it synchronous here

&#x20;       public IActionResult Login(\[FromBody] LoginModel model)

&#x20;       {

&#x20;           //Database call to check if the user exists and the password is correct

&#x20;           //var user = await \_userManager.FindByNameAsync(model.Username);

&#x20;           if (model.UserName == "admin" \&\& model.Password =="password123")

&#x20;           {

&#x20;               var token = GenerateToken(model);

&#x20;               return Ok(new { token });

&#x20;           }

&#x20;           return Unauthorized();

&#x20;       }

&#x20;       private string GenerateToken(LoginModel user)

&#x20;       {

&#x20;           var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(\_configuration\["JwtSettings:Secret"]));

&#x20;           var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



&#x20;           var claims = new\[] {

&#x20;           new Claim(JwtRegisteredClaimNames.Sub, user.UserName),

&#x20;           new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

&#x20;           new Claim("Name", user.UserName)

&#x20;       };



&#x20;           var token = new JwtSecurityToken(

&#x20;               \_configuration\["JwtSettings:Issuer"],

&#x20;               \_configuration\["JwtSettings:Audience"],

&#x20;               claims,

&#x20;               expires: DateTime.Now.AddMinutes(60),

&#x20;               signingCredentials: creds

&#x20;           );



&#x20;           return new JwtSecurityTokenHandler().WriteToken(token);

&#x20;       }

&#x20;   }

}



**productscontroller.cs**



using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

using MyApp.API.Interfaces;

using MyApp.API.Models;



namespace MyApp.API.Controllers

{

&#x20;   \[Route("api/\[controller]")]

&#x20;   \[ApiController]

&#x20;   public class ProductsController : ControllerBase

&#x20;   {

&#x20;       private readonly IProductService \_productService;

&#x20;       public ProductsController(IProductService productService)

&#x20;       {

&#x20;           \_productService = productService;

&#x20;       }



&#x20;       \[Authorize]

&#x20;       \[HttpGet]

&#x20;       public async Task<IActionResult> GetAllProductDetails()

&#x20;       {

&#x20;           var products = await \_productService.GetAllProductAsync();

&#x20;           if (products == null || products.Count() == 0)

&#x20;           {

&#x20;               return NotFound("No products founds");

&#x20;           }

&#x20;           return Ok(products);

&#x20;       }



&#x20;       // POST api/<ProductsController>

&#x20;       \[HttpPost]

&#x20;       public async Task<IActionResult> AddProductDetails(\[FromBody] Product product)

&#x20;       {

&#x20;           await \_productService.AddProductAsync(product);

&#x20;           return Created();



&#x20;       }



&#x20;   }

}







this is the sample code for JWT token implementation

use this sample code and implement JWT token in all 6 projects

