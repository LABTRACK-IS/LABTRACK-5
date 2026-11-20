import React, { useState, useEffect } from 'react';
import { Sparkles, Terminal } from 'lucide-react';

interface SequentialTypewriterBannerProps {
  phrases?: string[];
  className?: string;
  compact?: boolean;
}

const DEFAULT_LABTRACK_PHRASES = [
  'LABTRACK: An AI-Assisted Computer Laboratory Asset and Maintenance Management System',
  'Real-Time Workstation Telemetry • 2-Second Mobile QR Session Check-In • AES-256 Security',
  'Programmer / Developer: John Sven Morales • UI/UX Designer: Dhan Lester Igdao',
  'Project Manager: Mary Aprilieta Aligato • Database Designer: Aviegel Fernandez • System Analyst: Cora Cayetano',
];

export const SequentialTypewriterBanner: React.FC<SequentialTypewriterBannerProps> = ({
  phrases = DEFAULT_LABTRACK_PHRASES,
  className = '',
  compact = false,
}) => {
  const [phraseIndex, setPhraseIndex] = useState(0);
  const [displayedText, setDisplayedText] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    const currentPhrase = phrases[phraseIndex % phrases.length] || '';
    const speed = isDeleting ? 18 : 36;

    const timer = window.setTimeout(() => {
      if (!isDeleting && displayedText.length < currentPhrase.length) {
        setDisplayedText(currentPhrase.slice(0, displayedText.length + 1));
      } else if (!isDeleting && displayedText.length === currentPhrase.length) {
        window.setTimeout(() => setIsDeleting(true), 2200);
      } else if (isDeleting && displayedText.length > 0) {
        setDisplayedText(currentPhrase.slice(0, displayedText.length - 1));
      } else if (isDeleting && displayedText.length === 0) {
        setIsDeleting(false);
        setPhraseIndex((prev) => (prev + 1) % phrases.length);
      }
    }, speed);

    return () => window.clearTimeout(timer);
  }, [displayedText, isDeleting, phraseIndex, phrases]);

  return (
    <div
      className={`rounded-2xl bg-slate-950/85 backdrop-blur-xl border border-cyan-500/30 shadow-[0_12px_35px_rgba(0,0,0,0.45)] ${
        compact ? 'px-4 py-2.5' : 'px-5 py-3.5'
      } flex items-center gap-3 ${className}`}
    >
      <div className="p-1.5 rounded-xl bg-blue-600/20 border border-blue-400/40 text-cyan-400 shrink-0">
        <Terminal className={compact ? 'w-3.5 h-3.5' : 'w-4 h-4'} />
      </div>
      <div className="min-w-0 flex-1 font-mono text-left">
        <div className="flex items-center gap-1.5 text-[9px] font-black uppercase tracking-widest text-cyan-400">
          <Sparkles className="w-2.5 h-2.5 text-amber-400 animate-spin" />
          <span>JRMSU LABTRACK SYSTEM STREAM</span>
        </div>
        <p className={`${compact ? 'text-[11px]' : 'text-xs sm:text-sm'} font-bold text-white truncate`}>
          <span>{displayedText}</span>
          <span className="inline-block w-1.5 h-3.5 bg-cyan-400 ml-1 animate-pulse align-middle" />
        </p>
      </div>
    </div>
  );
};

export default SequentialTypewriterBanner;