namespace Decos.ServiceContext.Interfaces
{
  /// <summary>
  /// 
  /// </summary>
  public interface IServiceContext
  {
    IJoinApiService JoinApiService { get; }
    IAllyOdataService AllyODataService { get; }
  }
}