This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## User Login Information

This project contains two fully functional login systems with authentication:

### 1. Stock Receiving System (Manage Agent Stock Receiving)
- **URL:** `/stock-receiving-login`
- **Username:** `stock_admin`
- **Password:** `stock123`
- **Dashboard:** `/stock-receiving-dashboard`
- **Purpose:** Manage inventory and stock receiving operations

### 2. Delivery System (Delivery Plan)
- **URL:** `/delivery-login`
- **Username:** `delivery_admin`
- **Password:** `delivery123`
- **Dashboard:** `/delivery-dashboard`
- **Purpose:** Access delivery planning and logistics management

## Features

✅ **Complete Authentication System**
- Secure login with credential validation
- Session management with localStorage persistence
- Auto-redirect for authenticated users
- Protected routes with role-based access
- Proper logout functionality

✅ **Fully Functional Dashboards**
- Stock Receiving Dashboard with inventory management UI
- Delivery Dashboard with route planning interface
- Real-time status indicators and mock data
- Professional UI with consistent design

✅ **User Experience**
- Loading states and error handling
- Demo credentials shown on login forms
- Responsive design for all screen sizes
- Smooth animations and transitions

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
