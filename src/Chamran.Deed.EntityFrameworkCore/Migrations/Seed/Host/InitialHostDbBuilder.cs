﻿using Chamran.Deed.EntityFrameworkCore;

namespace Chamran.Deed.Migrations.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly DeedDbContext _context;

        public InitialHostDbBuilder(DeedDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create();

            _context.SaveChanges();
        }
    }
}
