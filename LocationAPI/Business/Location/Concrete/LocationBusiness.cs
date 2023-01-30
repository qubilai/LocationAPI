using Business.Location.Abstract;
using Cache.Abstract;
using Core.Abstract;
using Microsoft.Extensions.Configuration;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Business.Location.Concrete
{
    public class LocationBusiness : ILocationBusiness
    {
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _config;
        private readonly ICityRepository _cityRepo;
        private readonly IDistrictRepository _districtRepo;
        private readonly INeighborhoodRepository _neRepo;
        private readonly ILocationRepository _locationRepo;


        public LocationBusiness(ICacheService cacheService, IConfiguration configuration, IDistrictRepository districtRepo, ICityRepository cityRepo, INeighborhoodRepository neRepo, ILocationRepository locationRepo)
        {
            _districtRepo = districtRepo;
            _cacheService = cacheService;
            _config = configuration;
            _cityRepo = cityRepo;
            _neRepo = neRepo;
            _locationRepo = locationRepo;
        }

        public async Task<ResultModel<LocationModel>> Get()
        {
            IConfigurationSection url = _config.GetSection("CityUrl");
            ResultModel<LocationModel> resultModel = new ResultModel<LocationModel>();
            resultModel.Data = new List<LocationModel>();
            string key = "cityId_";
            string responseFromServer = SendRequest(url.Value);
            FeatureModel featureModel = JsonConvert.DeserializeObject<FeatureModel>(responseFromServer);
            foreach (var city in featureModel.features)
            {
                var redisKey = key + city.properties.id.ToString();
                var redisResult = SearchRedisResult(redisKey, city);
                //Rediste yok ise
                if (redisResult.Result == null)
                {         
                    var cityEntity = _cityRepo.Get(city.properties.id);
                    if (cityEntity.properties.id != 0)
                    {
                        //Veritabanında var ise
                        resultModel.Data.Add(cityEntity);
                    }
                    else
                    {
                        //Veritabanında yok ise
                        _cityRepo.Save(city);
                        resultModel.Data.Add(city);
                    }
                }
                else
                {
                    //Rediste var
                    resultModel.Data.Add(redisResult.Result);
                }
            }
            IsSuccessful(resultModel);
            return resultModel;
        }
        public async Task<ResultModel<LocationModel>> GetDistrict(int cityId)
        {
            var url = _config.GetSection("DistrictBaseUrl");
            ResultModel<LocationModel> resultModel = new ResultModel<LocationModel>();
            resultModel.Data = new List<LocationModel>();
            string requestUrl = $"{url.Value}/{cityId}";
            string key = "districtId_";
            string responseFromServer = SendRequest(requestUrl);
            FeatureModel featureModel = JsonConvert.DeserializeObject<FeatureModel>(responseFromServer);
            foreach (var district in featureModel.features)
            {
                var redisKey = key + district.properties.id.ToString();
                var redisResult = SearchRedisResult(redisKey, district);
                //Rediste yok ise
                if (redisResult.Result == null)
                {           
                    var districtEntity = _districtRepo.Get(district.properties.id);
                    if (districtEntity.properties.id != 0)
                    {
                        //Veritabanında var ise
                        resultModel.Data.Add(districtEntity);
                    }
                    else
                    {
                        //Veritabanında yok ise
                        district.properties.ilId = cityId;
                        _districtRepo.Save(district);
                        resultModel.Data.Add(district);
                    }
                }
                else
                {
                    //Rediste var
                    resultModel.Data.Add(redisResult.Result);
                }
            }
            IsSuccessful(resultModel);
            return resultModel;
        }
        public async Task<ResultModel<LocationModel>> GetNeighborhood(int districtId)
        {
            var url = _config.GetSection("NeighborhoodBaseUrl");
            ResultModel<LocationModel> resultModel = new ResultModel<LocationModel>();
            resultModel.Data = new List<LocationModel>();
            string requestUrl = $"{url.Value}/{districtId}";
            string key = "neighborhoodId_";
            string responseFromServer = SendRequest(requestUrl);
            FeatureModel featureModel = JsonConvert.DeserializeObject<FeatureModel>(responseFromServer);
            foreach (var neighborhood in featureModel.features)
            {
                string redisKey = key + neighborhood.properties.id.ToString();
                var redisResult = SearchRedisResult(redisKey, neighborhood);
                //Rediste yok ise
                if (redisResult.Result == null)
                {
                    var districtEntity = _neRepo.Get(neighborhood.properties.id);
                    if (districtEntity.properties.id != 0)
                    {
                        //Veritabanında var ise
                        resultModel.Data.Add(districtEntity);
                    }
                    else
                    {
                        //Veritabanında yok ise
                        neighborhood.properties.ilceId = districtId;
                        _neRepo.Save(neighborhood);
                        resultModel.Data.Add(neighborhood);
                    }
                }
                else
                {
                    //Rediste var
                    //LocationModel locationModel = JsonConvert.DeserializeObject<LocationModel>(redisResult);
                    resultModel.Data.Add(redisResult.Result);
                }
            }
            IsSuccessful(resultModel);
            return resultModel;
        }
        public async Task<ResultModel<LocationModel>> GetLocation(int neighborhoodId, int landNumber, int parcelNumber)
        {
            var url = _config.GetSection("ParcelBaseUrl");
            LocationModel locationModel = null;
            ResultModel<LocationModel> resultModel = new ResultModel<LocationModel>();
            resultModel.Data = new List<LocationModel>();
            string key = $"{neighborhoodId}_{landNumber}_{parcelNumber}";
            string urlParams = $"{neighborhoodId}/{landNumber}/{parcelNumber}";
            string requestUrl = url.Value + urlParams;
            var redisResult = await _cacheService.GetValueAsync(key);
            if (redisResult == null)
            {
                string responseFromServer = SendRequest(requestUrl);
                locationModel = JsonConvert.DeserializeObject<LocationModel>(responseFromServer);
                var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
                bool locationObject = await _cacheService.SetDataAsync<LocationModel>(key, locationModel, expirationTime);
                var locationEntity = _locationRepo.Get(neighborhoodId, landNumber, parcelNumber);
                if (locationEntity.properties.id != 0)
                {
                    //Veritabanında var ise
                    resultModel.Data.Add(locationEntity);
                }
                else
                {
                    //Veritabanında yok ise
                    _locationRepo.Save(locationModel);
                    resultModel.Data.Add(locationModel);
                }
            }
            else
            {
                locationModel = new LocationModel();
                locationModel = JsonConvert.DeserializeObject<LocationModel>(redisResult);
                resultModel.Data.Add(locationModel);
            }
            IsSuccessful(resultModel);
            return resultModel;
        }
        private string SendRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            String ver = response.ProtocolVersion.ToString();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseFromServer = reader.ReadToEnd();
            return responseFromServer;
        }
        private async Task<LocationModel> SearchRedisResult(string key, LocationModel model)
        {
            var redisResult = await _cacheService.GetValueAsync(key);
            LocationModel locationModel = null;
            //Rediste yok ise
            if (redisResult == null)
            {
                var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
                bool setData = await _cacheService.SetDataAsync<LocationModel>(key, model, expirationTime);
            }
            else
            {
                //Rediste var
                locationModel = JsonConvert.DeserializeObject<LocationModel>(redisResult);

            }
            return locationModel;
        }
        private void IsSuccessful(ResultModel<LocationModel> result)
        {
            if (result.Data.Count() == 0)
            {
                result.Mesaj = "İşlem başarısız";
            }
            else
            {
                result.Mesaj = "İşlem başarılı";

            }
        }

    }
}
