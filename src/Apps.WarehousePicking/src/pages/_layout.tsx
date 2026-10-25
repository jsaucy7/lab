import { useLocation, useNavigate, Outlet } from "react-router-dom";
import { BoxRegular, ArrowSwapRegular, LocationRegular } from "@fluentui/react-icons";
import { Tab, TabList } from "@fluentui/react-components";
import { useStyles } from "./_layout.styles";

const navigation = [
  { path: "/items", label: "Items", icon: <BoxRegular /> },
  { path: "/transactions", label: "Transactions", icon: <ArrowSwapRegular /> },
  { path: "/locations", label: "Locations", icon: <LocationRegular /> },
];

function selectedPath(pathname: string): string {
  if (pathname.startsWith("/transactions")) return "/transactions";
  if (pathname.startsWith("/locations")) return "/locations";
  return "/items";
}

export default function Layout() {
  const styles = useStyles();
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const selected = selectedPath(pathname);

  return (
    <div className={styles.root}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <div className={styles.brand}>
            <span className={styles.brandMark}><BoxRegular /></span>
            <span className={styles.brandName}>Warehouse Picking</span>
          </div>
          <TabList
            className={styles.tabs}
            selectedValue={selected}
            onTabSelect={(_, data) => navigate(String(data.value))}
          >
            {navigation.map((item) => (
              <Tab key={item.path} value={item.path} icon={item.icon}>{item.label}</Tab>
            ))}
          </TabList>
        </div>
      </header>
      <main className={styles.main}><Outlet /></main>
      <nav className={styles.bottomNav} aria-label="Warehouse sections">
        {navigation.map((item) => (
          <button
            key={item.path}
            type="button"
            className={styles.bottomItem}
            aria-current={selected === item.path ? "page" : undefined}
            onClick={() => navigate(item.path)}
          >
            <span className={styles.bottomIcon}>{item.icon}</span>
            <span>{item.label}</span>
          </button>
        ))}
      </nav>
    </div>
  );
}
