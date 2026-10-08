import React from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, Building2, MapPin, Star } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { ProjectListItem } from '../../types/project';
import { formatDevelopmentType, getStatusVariant } from '../../utils/formatters';
import Badge from '../ui/Badge';
import Card from '../ui/Card';
import { ImageWithFallback } from '../ui/ImageWithFallback';
import { resolveResourceUrl } from '../../utils/urlUtils';

interface ProjectCardProps {
  project: ProjectListItem;
}

export const ProjectCard: React.FC<ProjectCardProps> = ({ project }) => {
  const { t } = useTranslation(['projects', 'common']);
  const visibleTechs = project.technologies.slice(0, 3);
  const hiddenTechCount = project.technologies.length - visibleTechs.length;

  return (
    <Card className="project-card" clickable>
      {/* Visual Media Header */}
      <div className="project-card__media">
        <ImageWithFallback
          src={resolveResourceUrl(project.coverImageUrl)}
          alt={`${project.name} ${t('projects.card.coverImageSuffix', 'kapak görseli')}`}
          className="project-card__image"
        />
        <div className="project-card__badges-overlay">
          <Badge variant={getStatusVariant(project.status.code)} size="sm">
            {project.status.name}
          </Badge>
          {project.isFeatured && (
            <span className="project-card__featured-pill" title={t('projects.card.featuredProject', 'Öne Çıkan Proje')}>
              <Star size={12} fill="currentColor" aria-hidden="true" />
              <span>{t('projects.card.featured', 'Öne Çıkan')}</span>
            </span>
          )}
        </div>
      </div>

      {/* Card Content Body */}
      <div className="project-card__content">
        <div className="project-card__meta-top">
          <Badge variant="default" size="sm" className="project-card__category">
            {project.category.name}
          </Badge>
          <span className="project-card__dev-type">
            {formatDevelopmentType(project.developmentType, t)}
          </span>
        </div>

        <h3 className="project-card__title">
          <Link to={`/projects/${project.slug}`} className="project-card__title-link">
            {project.name}
          </Link>
        </h3>

        <p className="project-card__description">
          {project.shortDescription}
        </p>

        {/* Secondary Info: Team & Location */}
        <div className="project-card__secondary-info">
          {project.primaryTeam && (
            <span className="project-card__info-item" title={`${t('projects.card.primaryTeam', 'Sorumlu Ekip')}: ${project.primaryTeam.name}`}>
              <Building2 size={14} className="project-card__info-icon" aria-hidden="true" />
              <span>{project.primaryTeam.name}</span>
            </span>
          )}
          {project.locations && project.locations.length > 0 && (
            <span className="project-card__info-item" title={`${t('projects.card.location', 'Lokasyon')}: ${project.locations.map(l => l.name).join(', ')}`}>
              <MapPin size={14} className="project-card__info-icon" aria-hidden="true" />
              <span>{project.locations[0].name}{project.locations.length > 1 ? ` (+${project.locations.length - 1})` : ''}</span>
            </span>
          )}
        </div>

        {/* Technologies List */}
        {project.technologies.length > 0 && (
          <div className="project-card__tech-list" aria-label={t('projects.card.technologiesUsed', 'Kullanılan Teknolojiler')}>
            {visibleTechs.map((tech) => (
              <span key={tech.id} className="project-card__tech-tag">
                {tech.name}
              </span>
            ))}
            {hiddenTechCount > 0 && (
              <span className="project-card__tech-more" title={`+${hiddenTechCount} ${t('projects.card.otherTechnologies', 'diğer teknoloji')}`}>
                +{hiddenTechCount}
              </span>
            )}
          </div>
        )}

        {/* Card Footer Action */}
        <div className="project-card__footer">
          <Link
            to={`/projects/${project.slug}`}
            className="project-card__cta"
            aria-label={`${project.name} ${t('projects.card.inspectDetailsAria', 'projesinin detaylarını inceleyin')}`}
          >
            <span>{t('projects.card.inspectDetails', 'Detayları İncele')}</span>
            <ArrowRight size={16} aria-hidden="true" />
          </Link>
        </div>
      </div>
    </Card>
  );
};

export default ProjectCard;

