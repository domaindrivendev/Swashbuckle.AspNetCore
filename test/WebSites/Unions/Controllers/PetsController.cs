using Microsoft.AspNetCore.Mvc;

namespace Unions.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class PetsController : ControllerBase
{
    /// <summary>
    /// Gets a pet.
    /// </summary>
    /// <param name="id">The Id of the pet.</param>
    [HttpGet("{id}", Name = "GetPet")]
    public Pet GetPet(int id)
        => id % 2 == 0 ? new Cat("Whiskers", 9) : new Dog("Rex", true);

    /// <summary>
    /// Creates a pet.
    /// </summary>
    /// <param name="pet">The pet to create.</param>
    [HttpPost(Name = "CreatePet")]
    [ProducesResponseType<Pet>(StatusCodes.Status201Created)]
    public IActionResult CreatePet([FromBody] Pet pet)
        => CreatedAtRoute("GetPet", new { id = 1 }, pet);

    /// <summary>
    /// Gets the owner of some pets.
    /// </summary>
    /// <param name="id">The Id of the owner.</param>
    [HttpGet("owners/{id}", Name = "GetOwner")]
    public Owner GetOwner(int id)
        => new("Jane", new Cat("Whiskers", 9), null, [new Dog("Rex", true)]);

    /// <summary>
    /// Gets a JSON value that may contain nested values.
    /// </summary>
    [HttpGet("values", Name = "GetValue")]
    public NestedValue GetValue()
        => new NestedValue[] { 1, "two", new NestedValue[] { 3 } };
}

/// <summary>
/// A cat.
/// </summary>
/// <param name="Name">The name of the cat.</param>
/// <param name="Lives">The number of lives the cat has left.</param>
public record Cat(string Name, int Lives);

/// <summary>
/// A dog.
/// </summary>
/// <param name="Name">The name of the dog.</param>
/// <param name="GoodBoy">Whether the dog is a good boy.</param>
public record Dog(string Name, bool GoodBoy);

/// <summary>
/// A pet, which is either a cat or a dog.
/// </summary>
public union Pet(Cat, Dog);

/// <summary>
/// A value that is either a number, a string or an array of values.
/// </summary>
public union NestedValue(int, string, NestedValue[]);

/// <summary>
/// The owner of some pets.
/// </summary>
/// <param name="Name">The name of the owner.</param>
/// <param name="Pet">The owner's current pet.</param>
/// <param name="PreviousPet">The owner's previous pet, if any.</param>
/// <param name="OtherPets">The owner's other pets.</param>
public record Owner(string Name, Pet Pet, Pet? PreviousPet, IList<Pet> OtherPets);
