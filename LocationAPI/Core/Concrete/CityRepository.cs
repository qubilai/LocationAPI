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
    public class CityRepository:ICityRepository
    {
        private readonly ApplicationContext _context;
        public CityRepository(ApplicationContext context)
        {
            _context = context;
        }
        public LocationModel Get(int id)
        {
            LocationModel location = new LocationModel();
            location.properties = new PropertyModel();
            location.geometry = new GeometryModel();
            try
            {
                City model = _context.City.Where(x => x.Id == id).FirstOrDefault();
                if (model != null)
                {
                    location.properties.text = model.Name;
                    location.properties.id = model.Id;
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
                City city = new City();
                try
                {
                    city.Name = model.properties.text;
                    city.Id = model.properties.id;
                    _context.City.Add(city);
                    _context.SaveChanges();
                    dbContextTransaction.Commit();

                }
                catch (Exception ex)
                {
                    throw ex;
                    dbContextTransaction.Rollback();
                }
                return true;
            }
        }
    }
}
