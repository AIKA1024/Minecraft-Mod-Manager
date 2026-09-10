namespace MAZDA_MCTool.Abstraction.Contracts.Models;

public interface IModProjectNetInfo
{
  public string Id { get; set; }
  public string Slug { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
  /// <summary>
  /// The client side support of the project
  /// </summary>
  // public string Status { get; set; }
  public string IconUrl { get; set; }
}