import React from 'react';
import Skeleton from '../ui/Skeleton';

interface ProjectGridSkeletonProps {
  count?: number;
}

export const ProjectGridSkeleton: React.FC<ProjectGridSkeletonProps> = ({ count = 6 }) => {
  return (
    <div className="project-grid" aria-label="Projeler yükleniyor..." aria-busy="true">
      {Array.from({ length: count }).map((_, idx) => (
        <div key={idx} className="project-card project-card--skeleton" aria-hidden="true">
          <Skeleton variant="rect" height={160} className="project-card__skeleton-media" />
          <div className="project-card__content">
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
              <Skeleton variant="rect" width={80} height={20} />
              <Skeleton variant="rect" width={100} height={20} />
            </div>
            <div style={{ marginBottom: '8px' }}>
              <Skeleton variant="text" width="85%" height={22} />
            </div>
            <div style={{ marginBottom: '4px' }}>
              <Skeleton variant="text" width="100%" height={14} />
            </div>
            <div style={{ marginBottom: '16px' }}>
              <Skeleton variant="text" width="70%" height={14} />
            </div>
            <div style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
              <Skeleton variant="rect" width={60} height={22} />
              <Skeleton variant="rect" width={70} height={22} />
              <Skeleton variant="rect" width={50} height={22} />
            </div>
            <Skeleton variant="rect" width="100%" height={36} />
          </div>
        </div>
      ))}
    </div>
  );
};

export default ProjectGridSkeleton;
