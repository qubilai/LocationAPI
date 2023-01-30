using Business.Location.Abstract;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace LocationAPI.Controllers
{
    [Route("api/Location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationBusiness _locationService;
        public LocationController(ILocationBusiness locationService)
        {
            _locationService = locationService;
        }
        [HttpGet]
        public async Task<ActionResult<ResultModel<LocationModel>>> GetCity()
        {
            Task<ResultModel<LocationModel>> locationModel = null;
            try
            {
                locationModel = _locationService.Get();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return await locationModel;
        }
        [HttpGet("GetDistrict/{id}")]
        public async Task<ActionResult<ResultModel<LocationModel>>> GetDistrict(int id)
        {
            Task<ResultModel<LocationModel>> locationModel = null;
            try
            {
                locationModel = _locationService.GetDistrict(id);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return await locationModel;
        }
        [HttpGet("GetNeighborhood/{id}")]
        public async Task<ActionResult<ResultModel<LocationModel>>> GetNeighborhood(int id)
        {
            Task<ResultModel<LocationModel>> locationModel = null;

            try
            {
                locationModel = _locationService.GetNeighborhood(id);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return await locationModel;
        }
        [HttpGet("GetLocation/{neighborhoodId}/{landNumber}/{parcelNumber}")]
        public async Task<ActionResult<ResultModel<LocationModel>>> GetLocation(int neighborhoodId,int landNumber,int parcelNumber)
        {
            Task<ResultModel<LocationModel>> locationModel = null;
            try
            {
                locationModel = _locationService.GetLocation(neighborhoodId, landNumber, parcelNumber);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return await locationModel;
        }
    }
}
