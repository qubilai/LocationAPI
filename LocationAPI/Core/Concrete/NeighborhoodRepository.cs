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
    public class NeighborhoodRepository: INeighborhoodRepository
    {
        private readonly ApplicationContext _context;
        public NeighborhoodRepository(ApplicationContext context)
        {
            _context = context;
        }
        public LocationModel Get(int id)
        {
            LocationModel location = new LocationModel();
            location.properties = new PropertyModel();
            try
            {
                Neighborhood model = _context.Neighborhood.Where(x => x.Id == id).FirstOrDefault();
                if (model != null)
                {
                    location.properties.text = model.Name;
                    location.properties.id = model.Id;
                    location.properties.ilceId = model.DistrictId.HasValue ? model.DistrictId.Value : 0;
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
                Neighborhood neighborhood = new Neighborhood();
                try
                {
                    neighborhood.Name = model.properties.text;
                    neighborhood.Id = model.properties.id;
                    neighborhood.DistrictId = model.properties.ilceId;
                    _context.Neighborhood.Add(neighborhood);
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
