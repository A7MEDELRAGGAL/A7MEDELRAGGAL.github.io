(function () {
  const defaultContent = {
    version: 1,
    profile: {
      name: "Ahmed Mohammed Saad El-Raggal",
      headline: "Information Security Analyst | IT & Network Infrastructure",
      location: "Alexandria, Egypt",
      email: "ahmedxd444@gmail.com",
      phone: "+201025344300"
    },
    projects: [
      {
        title: "Taheel",
        description: "Assistive platform concept focused on practical accessibility workflows and digital inclusion.",
        image: "images/projects/blog.png",
        category: "web",
        tech: "Web Platform",
        url: "https://github.com/A7MEDELRAGGAL"
      },
      {
        title: "Menasa+",
        description: "Portfolio and growth platform direction for showcasing technical work, services, and impact.",
        image: "images/projects/network.jpg",
        category: "web",
        tech: "Platform",
        url: "https://github.com/A7MEDELRAGGAL"
      },
      {
        title: "Golden Touch",
        description: "Business-facing digital presence work focused on clear UX and reliable content structure.",
        image: "images/projects/dentist.jpg",
        category: "web",
        tech: "Frontend",
        url: "https://github.com/A7MEDELRAGGAL"
      }
    ],
    experiences: [
      {
        role: "Cybersecurity Instructor",
        company: "Freelance",
        period: "2024 — Present",
        bullets: [
          "Delivered practical networking and security training sessions.",
          "Mentored students through lab-based cybersecurity projects."
        ]
      }
    ],
    testimonials: [
      {
        quote: "Ahmed combines hands-on execution with strong mentoring and communication.",
        name: "Professional Recommendation",
        title: "Cybersecurity & IT"
      }
    ],
    images: {
      projects: [
        "images/projects/currency.jpg",
        "images/projects/cane.jpg",
        "images/projects/buggedcart.jpg"
      ],
      certificates: [
        "images/certs/ccna.jpg",
        "images/certs/ccnp.jpg",
        "images/certs/cyberops.jpg"
      ],
      gallery: [
        "images/gallery/photo1.jpg",
        "images/gallery/photo2.jpg",
        "images/gallery/photo3.jpg"
      ]
    }
  };

  function clone(value) {
    return JSON.parse(JSON.stringify(value));
  }

  if (!window.DEFAULT_SITE_CONTENT || typeof window.DEFAULT_SITE_CONTENT !== "object") {
    window.DEFAULT_SITE_CONTENT = clone(defaultContent);
  }

  if (!window.SITE_CONTENT || typeof window.SITE_CONTENT !== "object") {
    window.SITE_CONTENT = clone(defaultContent);
  }
})();
