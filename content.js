/* ============================================================
   SITE CONTENT — single source of truth for admin.html
   - index.html works WITHOUT this file (static fallback).
   - admin.html reads/writes this shape to localStorage key: site_cms_v1
   - To publish: export from admin.html and replace this file,
     or just keep using localStorage on your own browser.
   ============================================================ */
window.SITE_DEFAULTS = {
  version: 1,
  profile: {
    name: "Ahmed Mohammed Saad El-Raggal",
    title: "IT Engineer — Robotics, AI, Web & Cyber Projects",
    email: "ahmedxd444@gmail.com",
    phone: "01025344300",
    whatsapp: "201025344300",
    location: "Alexandria, Egypt",
    availability: true,
    cvUrl: "Ahmed-El-Raggal-CV.pdf"
  },
  // Items added here appear IN ADDITION to the static HTML (additive, safe).
  projects: [],
  experiences: [],
  testimonials: [],
  // Example shape (do not delete — copy/paste in admin):
  // { title: "My New Project", category: "web", tags: "React, Tailwind", description: "...", url: "https://...", icon: "fa-rocket" }
  // { title: "Job Title", org: "Company", period: "2026 — Present", details: "What you did..." }
  // { quote: "...", name: "Name", role: "Role" }
  imagesManifest: [
    "images/logo.png",
    "images/profile.webp",
    "images/projects/currency.jpg",
    "images/projects/cane.jpg",
    "images/projects/buggedcart.jpg",
    "images/projects/password.jpg",
    "images/projects/network.jpg",
    "images/projects/fire.jpg",
    "images/projects/watertank.jpg",
    "images/projects/cnc.jpg",
    "images/projects/shiftregister.jpg",
    "images/projects/dentist.jpg",
    "images/projects/blog.png"
  ]
};
