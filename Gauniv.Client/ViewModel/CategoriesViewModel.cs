﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Model;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using Gauniv.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gauniv.Client.ViewModel
{
    public partial class CategoriesViewModel : ObservableObject
    {


        [ObservableProperty]
        private ObservableCollection<Model.CategoryDto> categoryDtos = new(); // Liste des jeux

        // Constructeur
        public CategoriesViewModel()
        {
            GetCategories(); // Récupération des jeux
        }


        public async Task GetCategories()
        {
            try
            {
                Task.Run(async () =>
                {
                    var lastCategoryDtos = await NetworkService.Instance.GetCategoryList();
                    categoryDtos.Clear();
                    foreach (var c in lastCategoryDtos)
                    {
                        Model.CategoryDto categoryDto = new Model.CategoryDto
                        {
                            Id = c.Id,
                            Name = c.Name
                        };
                        categoryDtos.Add(categoryDto);
                    }
                });
                var test = categoryDtos;
                Console.WriteLine("Données récupérées avec succès !");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Erreur API : {ex.Message}");
            }
        }
    }
}
