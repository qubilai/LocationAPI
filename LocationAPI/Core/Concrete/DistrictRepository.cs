using Core.Abstract;
using DataAccess.Context;
using DataAccess.Entity;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Concrete
{
    public class DistrictRepository: IDistrictRepository
    {
        private readonly ApplicationContext _context;
        public DistrictRepository(ApplicationContext context)
        {
            _context = context;
        }
        public LocationModel Get(int id)
        {
            LocationModel location = new LocationModel();
            location.properties = new PropertyModel();
            try
            {
                District model = _context.District.Where(x => x.Id == id).FirstOrDefault();
                if (model != null)
                {
                    location.properties.text = model.Name;
                    location.properties.id = model.Id;
                    location.properties.ilId = model.CityId.HasValue ? model.CityId.Value : 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return location;
        }
        public bool Save(LocationModel model)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                District district = new District();
                try
                {
                    district.Name = model.properties.text;
                    district.Id = model.properties.id;
                    district.CityId = model.properties.ilId;
                    _context.District.Add(district);
                    _context.SaveChanges();
                    dbContextTransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw ex;

                }
                return true;
            }
        }
    }
}
