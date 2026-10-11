export function AuthFooter() {
  return (
    <footer className="auth-footer footer">
      <nav className="pull-left">
        <ul>
          <li>
            <a href="#about">About BugShield</a>
          </li>
          <li>
            <a href="#terms">Terms & Conditions</a>
          </li>
          <li>
            <a href="#company">Company</a>
          </li>
          <li>
            <a href="/support">Support</a>
          </li>
        </ul>
      </nav>
      <p className="copyright pull-right">
        © {new Date().getFullYear()} <a href="#company">BUGSKAN CyberSecurity Solutions</a> LLP.
      </p>
    </footer>
  );
}
