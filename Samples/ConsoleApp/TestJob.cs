using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.Data;
using ConsoleApp.Entities;
using ConsoleApp.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sillycore.BackgroundProcessing;

namespace ConsoleApp
{
    public class TestJob : IJob
    {
        private readonly SomeHelper _helper;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public TestJob(SomeHelper helper, IConfiguration configuration, DataContext context)
        {
            _helper = helper;
            _configuration = configuration;
            _context = context;
        }

        public async Task Run()
        {
            List<Sample> samples = await _context.Samples.ToListAsync();

            _context.Samples.Remove(samples.First());

            await _context.SaveChangesAsync();

            await Console.Out.WriteLineAsync(_configuration["TestConfig"]);
        }
    }
}