using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerShortResponse>>> GetCustomersAsync()
    {
        var customers = await customerRepository.GetAllAsync();
        var response = customers.Select(c => new CustomerShortResponse
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email
        }).ToList();

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null) return NotFound();

        var response = new CustomerResponse
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Preferences = customer.CustomerPreferences?.Select(cp => new PreferenceResponse
            {
                Id = cp.Preference.Id,
                Name = cp.Preference.Name
            }).ToList(),
            Promocodes = customer.PromoCodes?.Select(pc => new PromoCodeShortResponse
            {
                Id = pc.Id,
                Code = pc.Code,
                ServiceInfo = pc.ServiceInfo,
                BeginDate = pc.BeginDate,
                EndDate = pc.EndDate
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomerAsync([FromBody] CreateOrEditCustomerRequest request)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };

        if (request.PreferenceIds != null && request.PreferenceIds.Any())
        {
            var prefs = await preferenceRepository.GetAllAsync();
            var selected = prefs.Where(p => request.PreferenceIds.Contains(p.Id)).ToList();
            customer.CustomerPreferences = selected.Select(s => new CustomerPreference
            {
                CustomerId = customer.Id,
                PreferenceId = s.Id
            }).ToList();
        }

        await customerRepository.AddAsync(customer);

        return CreatedAtAction(nameof(GetCustomerAsync), new { id = customer.Id }, null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditCustomersAsync(Guid id, [FromBody] CreateOrEditCustomerRequest request)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null) return NotFound();

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;

        customer.CustomerPreferences.Clear();
        if (request.PreferenceIds != null && request.PreferenceIds.Any())
        {
            var prefs = await preferenceRepository.GetAllAsync();
            var selected = prefs.Where(p => request.PreferenceIds.Contains(p.Id)).ToList();
            foreach (var s in selected)
            {
                customer.CustomerPreferences.Add(new CustomerPreference
                {
                    CustomerId = customer.Id,
                    PreferenceId = s.Id
                });
            }
        }

        await customerRepository.UpdateAsync(customer);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null) return NotFound();

        await customerRepository.DeleteAsync(customer);
        return NoContent();
    }
}