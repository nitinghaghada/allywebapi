using Decos.ServiceContext.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Decos.ServiceContext
{
  /// <summary>
  /// 
  /// </summary>
  public class ServiceContext :IServiceContext
  {
    private IConfiguration _configuration;
    private IJoinApiService _joinApiService;

    public ServiceContext(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    /// <summary>
    /// 
    /// </summary>
    public IJoinApiService JoinApiService => _joinApiService ?? (_joinApiService = new JoinApiService.JoinApiService(_configuration));

    /// <summary>
    /// 
    /// </summary>
    public IAllyOdataService AllyODataService => new AllyOdataService.AllyOdataService(_configuration);
  }
}