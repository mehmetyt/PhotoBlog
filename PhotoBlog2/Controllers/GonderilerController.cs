using Microsoft.AspNetCore.Mvc;
using PhotoBlog2.Data;
using PhotoBlog2.Models;

namespace PhotoBlog2.Controllers
{
	public class GonderilerController : Controller
	{
		private readonly UygulamaDbContext _db;
		private readonly IWebHostEnvironment _env;
		public GonderilerController(UygulamaDbContext db, IWebHostEnvironment env)
		{
			_db = db;
			_env = env;
		}
		public IActionResult Yeni()
		{
			return View();
		}
		[HttpPost, ValidateAntiForgeryToken]

		public IActionResult Yeni(YeniGonderiViewModel vm)
		{
			if (ModelState.IsValid)
			{
				string ext = Path.GetExtension(vm.Resim.FileName);
				string yeniDosyaAd = Guid.NewGuid() + ext;
				string yol = Path.Combine(_env.WebRootPath, "img", "upload", yeniDosyaAd);

				using (var fs = new FileStream(yol, FileMode.CreateNew))
				{
					vm.Resim.CopyTo(fs);
				}

				_db.Gonderiler.Add(new Gonderi()
				{
					Baslik = vm.Baslik,
					ResimYolu = yeniDosyaAd
				});

				_db.SaveChanges();

				return RedirectToAction("Index", "Home", new { Sonuc = "Eklendi" });
			}

			return View();
		}

		public IActionResult Guncelle(int id)
		{
			Gonderi gonderi = _db.Gonderiler.Find(id);
			if (gonderi == null)
				return NotFound();


			return View(new GuncelleGonderiViewModel()
			{
				Id = gonderi.Id,
				Baslik = gonderi.Baslik,
			});
		}
		[HttpPost]
		public IActionResult Guncelle(GuncelleGonderiViewModel vm)
		{
			if (ModelState.IsValid)
			{
				Gonderi gonderi = _db.Gonderiler.Find(vm.Id)!;
				if (gonderi == null)
					return NotFound();
				gonderi.Baslik = vm.Baslik;

				if (vm.Resim != null)
				{
					string eskiResim = gonderi.ResimYolu;

					string ext = Path.GetExtension(vm.Resim.FileName);
					string yeniDosyaAd = Guid.NewGuid() + ext;

					gonderi.ResimYolu = yeniDosyaAd;

					string yol = Path.Combine(_env.WebRootPath, "img", "upload", yeniDosyaAd);

					using (var fs = new FileStream(yol, FileMode.CreateNew))
					{
						vm.Resim.CopyTo(fs);
					}
					string dosya = Path.Combine(_env.WebRootPath, "img", "upload", eskiResim);
					System.IO.File.Delete(dosya);

				}

				_db.Update(gonderi);
				_db.SaveChanges();

				return RedirectToAction("Index", "Home", new { Sonuc = "Guncellendi" });
			}

			return View();
		}

		public IActionResult Sil(int id)
		{
			return View(_db.Gonderiler.Find(id));
		}
		[HttpPost]
		public IActionResult Sil(Gonderi vm)
		{
			Gonderi gonderi = _db.Gonderiler.Find(vm.Id);
			if (gonderi == null)
				return NotFound();

			if (gonderi != null)
			{
				string dosya = Path.Combine(_env.WebRootPath, "img", "upload", gonderi.ResimYolu);
				System.IO.File.Delete(dosya);

				_db.Gonderiler.Remove(gonderi);
				_db.SaveChanges();
				return RedirectToAction("Index", "Home", new { Sonuc = "Silindi" });
			}

			return View();
		}
	}
}
