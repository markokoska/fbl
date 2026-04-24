import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

export default function Navbar() {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="bg-slate-800 border-b border-slate-700">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          <div className="flex items-center gap-6">
            <Link to="/" className="text-xl font-bold text-emerald-400">
              FBL
            </Link>
            {user && (
              <>
                <Link to="/my-team" className="text-slate-300 hover:text-white text-sm">
                  My Team
                </Link>
                <Link to="/transfers" className="text-slate-300 hover:text-white text-sm">
                  Transfers
                </Link>
                <Link to="/leagues" className="text-slate-300 hover:text-white text-sm">
                  Leagues
                </Link>
                <Link to="/matches" className="text-slate-300 hover:text-white text-sm">
                  Matches
                </Link>
                <Link to="/standings" className="text-slate-300 hover:text-white text-sm">
                  Standings
                </Link>
                <Link to="/stats" className="text-slate-300 hover:text-white text-sm">
                  Stats
                </Link>
                <Link to="/injuries" className="text-slate-300 hover:text-white text-sm">
                  Injuries
                </Link>
                <Link to="/gameweek" className="text-slate-300 hover:text-white text-sm">
                  Live
                </Link>
                {isAdmin && (
                  <Link to="/admin" className="text-amber-400 hover:text-amber-300 text-sm">
                    Admin
                  </Link>
                )}
              </>
            )}
          </div>
          <div className="flex items-center gap-4">
            {user ? (
              <>
                <span className="text-slate-400 text-sm">{user.displayName}</span>
                <button
                  onClick={handleLogout}
                  className="text-slate-400 hover:text-white text-sm"
                >
                  Logout
                </button>
              </>
            ) : (
              <Link to="/login" className="text-slate-300 hover:text-white text-sm">
                Login
              </Link>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}
