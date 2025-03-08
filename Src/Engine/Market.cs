using System;
using System.Collections.Generic;
using System.Linq;

namespace UEconomy.Engine;

public class Market
{
    private Dictionary<string, float> _prices = new();
    private Dictionary<string, List<TradeOffer>> _supplies = new();
    private Dictionary<string, List<TradeOffer>> _demands = new();
    private Dictionary<string, float> _lastTradedAmount = new();

    public void Initialize(IEnumerable<string> resources)
    {
        foreach (var resource in resources)
        {
            _prices[resource] = 20.0f;
            _supplies[resource] = new List<TradeOffer>();
            _demands[resource] = new List<TradeOffer>();
            _lastTradedAmount[resource] = 0;
        }
    }

    public void ResetOffers()
    {
        foreach (var resource in _supplies.Keys)
        {
            _supplies[resource].Clear();
            _demands[resource].Clear();
        }
    }

    public void AddSupply(string resource, float amount, float minPrice, object seller)
    {
        if (_supplies.TryGetValue(resource, out var supply))
        {
            supply.Add(new TradeOffer { Amount = amount, Price = minPrice, Party = seller });
        }
    }

    public void AddDemand(string resource, float amount, float maxPrice, object buyer)
    {
        if (_demands.TryGetValue(resource, out var demand))
        {
            demand.Add(new TradeOffer { Amount = amount, Price = maxPrice, Party = buyer });
        }
    }

    public float GetPrice(string resource)
    {
        return _prices.GetValueOrDefault(resource, 0);
    }

    public string[] GetAllResources()
    {
        return _prices.Keys.ToArray();
    }

    public float GetSupplyDemandRatio(string resource)
    {
        var supply = _supplies.GetValueOrDefault(resource)?.Count ?? 0;
        var demand = _demands.GetValueOrDefault(resource)?.Count ?? 0;

        if (demand == 0)
        {
            return supply > 0 ? 2.0f : 1.0f;
        }

        if (supply == 0)
        {
            return 0.5f;
        }

        return (float) supply / demand;
    }

    public float GetLastTradedAmount(string resource)
    {
        return _lastTradedAmount.GetValueOrDefault(resource, 0);
    }

    public void ResolveTrades()
    {
        foreach (var resource in _prices.Keys.Where(resource => _supplies.ContainsKey(resource) && _demands.ContainsKey(resource)))
        {
            _supplies[resource].Sort((a, b) => a.Price.CompareTo(b.Price));
            _demands[resource].Sort((a, b) => b.Price.CompareTo(a.Price));

            float totalTraded = 0;
            float totalValue = 0;

            while (_supplies[resource].Count > 0 && _demands[resource].Count > 0)
            {
                var supply = _supplies[resource][0];
                var demand = _demands[resource][0];

                if (supply.Price <= demand.Price)
                {
                    var tradeAmount = Math.Min(supply.Amount, demand.Amount);
                    var tradePrice = (supply.Price + demand.Price) / 2;

                    totalTraded += tradeAmount;
                    totalValue += tradeAmount * tradePrice;

                    supply.Amount -= tradeAmount;
                    demand.Amount -= tradeAmount;

                    if (supply.Party is Province sellerProvince && demand.Party is Population buyerPop)
                    {
                        if (sellerProvince.Resources.ContainsKey(resource))
                        {
                            buyerPop.Wealth -= tradeAmount * tradePrice;
                        }
                    }

                    if (supply.Amount <= 0)
                    {
                        _supplies[resource].RemoveAt(0);
                    }

                    if (demand.Amount <= 0)
                    {
                        _demands[resource].RemoveAt(0);
                    }
                }
                else
                {
                    break;
                }
            }

            if (totalTraded > 0)
            {
                var newPrice = totalValue / totalTraded;
                _prices[resource] = _prices[resource] * 0.7f + newPrice * 0.3f;
                _lastTradedAmount[resource] = totalTraded;
            }
            else
            {
                if (_supplies[resource].Count > _demands[resource].Count)
                {
                    _prices[resource] *= 0.95f;
                }
                else if (_supplies[resource].Count < _demands[resource].Count)
                {
                    _prices[resource] *= 1.05f;
                }
            }
        }
    }
}