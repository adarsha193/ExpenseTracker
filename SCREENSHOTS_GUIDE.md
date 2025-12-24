# Screenshots Guide

## 📸 How to Add Screenshots to README

The README has been updated with placeholder references to screenshots. Follow these steps to add your screenshots:

### Step 1: Save Screenshots in Correct Folder

All screenshots should be saved in the `/screenshots` folder with these exact filenames:

```
screenshots/
├── 01-login.png              # Login Screen
├── 02-register.png           # Create Account / Registration
├── 03-password-reset.png     # Password Reset / Forgot Password
├── 04-onboarding.png         # Welcome / Onboarding Screen
├── 05-dashboard.png          # Dashboard - Main Screen
├── 06-all-expenses.png       # View All Expenses
├── 07-add-expense.png        # Add New Expense
├── 08-monthly-budget.png     # Monthly Budget & Alerts
├── 09-investments.png        # Investment Tracking
├── 10-salary.png             # Salary Management
├── 11-profile.png            # User Profile
└── 12-settings.png           # Settings & Preferences
```

### Step 2: Screenshot Naming Convention

| Filename | Screen Name | Purpose |
|----------|------------|---------|
| `01-login.png` | Login Screen | User authentication |
| `02-register.png` | Create Account | New user registration |
| `03-password-reset.png` | Password Reset | Account recovery |
| `04-onboarding.png` | Welcome/Onboarding | App introduction |
| `05-dashboard.png` | Dashboard | Main app interface |
| `06-all-expenses.png` | All Expenses | Expense history |
| `07-add-expense.png` | Add Expense | Create new expense |
| `08-monthly-budget.png` | Monthly Budget | Budget tracking |
| `09-investments.png` | Investment Tracking | Portfolio management |
| `10-salary.png` | Salary Management | Income tracking |
| `11-profile.png` | User Profile | Profile information |
| `12-settings.png` | Settings | App preferences |

### Step 3: Save Screenshots

Using your Android emulator, iOS simulator, or physical device:

1. **Take Screenshot:**
   - Android: Press Volume Down + Power button, or use `adb shell screencap -p > screenshot.png`
   - iOS: Use standard device screenshot method
   - Windows/macOS: Use platform-specific screenshot tools

2. **Resize if needed:**
   - Recommended: 500x1000px (portrait orientation)
   - Aspect ratio: 1:2 (phone-like proportions)
   - Format: PNG (preferred for quality)

3. **Save to Screenshots Folder:**
   ```bash
   cp /path/to/screenshot.png /Users/adarshahebbar/Documents/Maui.net/ExpenseTracker/screenshots/01-login.png
   ```

### Step 4: Verify in README

The README automatically displays screenshots from the `/screenshots` folder. Once you add images:

1. The gallery will appear in the **Screenshots Gallery** section
2. Images are organized in thematic groups (Authentication, Dashboard, etc.)
3. Each screenshot has a title and description

### Example: How Images Appear in README

```markdown
### 🔐 Authentication

| Login Screen | Create Account | Password Reset |
|---|---|---|
| ![Login](screenshots/01-login.png) | ![Register](screenshots/02-register.png) | ![Reset](screenshots/03-password-reset.png) |
```

---

## 📁 Current Screenshot Placeholders in README

The README includes these sections with image placeholders:

### 1. **Authentication Section** (Lines 460-485)
- Login Screen
- Create Account
- Password Reset

### 2. **Dashboard & Expenses Section** (Lines 487-503)
- Welcome Screen
- Dashboard
- All Expenses

### 3. **Add Expense & Budget Section** (Lines 505-519)
- Add Expense
- Monthly Budget

### 4. **Investments & Salary Section** (Lines 521-535)
- Investment Tracking
- Salary Management

### 5. **Profile & Settings Section** (Lines 537-551)
- User Profile
- Settings

---

## 🎨 Screenshot Best Practices

### For Better Looking Screenshots:

1. **Clean Device State**
   - Close background apps
   - Clear notifications
   - Use consistent user data

2. **Consistent Theming**
   - Use same theme (light/dark) for all screenshots
   - Consistent UI colors and fonts
   - Proper aspect ratio

3. **Data Quality**
   - Use realistic sample data
   - Show meaningful transactions
   - Include examples of features

4. **Documentation**
   - Screenshot captions explain features
   - Highlight key UI elements
   - Show user workflows

---

## 🚀 GitHub README Optimization

Once screenshots are added:

1. **README Appearance:**
   - Visual appeal increases engagement
   - Users quickly understand features
   - Better GitHub ranking

2. **File Organization:**
   ```
   ExpenseTracker/
   ├── README.md              # Main documentation with screenshot references
   ├── screenshots/           # All app screenshots
   │   ├── 01-login.png
   │   ├── 02-register.png
   │   └── ... (all 12 screenshots)
   └── docs/                  # Other documentation
   ```

3. **GitHub Display:**
   - Screenshots render automatically in README preview
   - Relative paths work for GitHub display
   - Images appear in dark/light mode (if properly formatted)

---

## 📝 Quick Checklist

- [ ] Create `/screenshots` folder ✅ (Already done)
- [ ] Capture 12 app screenshots
- [ ] Name files according to convention (01-login.png, etc.)
- [ ] Save to `/screenshots` folder
- [ ] Verify images appear in README preview
- [ ] Commit to GitHub: `git add screenshots/ && git commit -m "Add app screenshots"`
- [ ] Push to GitHub: `git push`

---

## ✅ README Sections Added

The README now includes:

1. **📸 Screenshots Gallery** section with:
   - Organized table layout
   - All 12 app screens
   - Grouped by feature (Authentication, Dashboard, Budget, etc.)
   - Descriptive captions

2. **🎨 Feature Overview** section with:
   - Visual feature checklist
   - ASCII diagram showing app structure
   - Quick reference for all features

3. **🎯 Quick Feature Overview** section with:
   - ASCII art feature tree
   - Complete feature breakdown
   - Visual representation of app capabilities

---

## 📞 Support

If you have questions about:
- **Screenshot capture**: See platform-specific guides above
- **README layout**: Check lines 460-551 in README.md
- **Image paths**: Ensure files are in `/screenshots` folder with exact names
- **GitHub display**: Test by viewing your GitHub repository

---

**Status**: ✅ README prepared and ready for screenshots  
**Next Step**: Capture screenshots and save to `screenshots/` folder  
**Timeline**: Screenshots will display automatically once files are added
