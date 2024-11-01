package com.drunkengames.hangover.domain.rank.service;

import com.drunkengames.hangover.domain.rank.dto.RankDto;
import com.drunkengames.hangover.domain.rank.dto.RankRequestDto;
import com.drunkengames.hangover.domain.rank.dto.RankResponseDto;
import com.drunkengames.hangover.domain.rank.repository.RankRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

/**
 * <pre>
 *     랭킹 관리 서비스 구현 클래스
 * </pre>
 *
 * @author 박봉균
 * @since JDK17 Eclipse Temurin
 */

@Service
@RequiredArgsConstructor
@Transactional(readOnly = true)
public class RankServiceImpl implements RankService {
    private final RankRepository rankRepository;

    @Transactional
    public void insertRank(RankRequestDto rankRequestDto) throws Exception {
        rankRepository.insertRank(rankRequestDto);
    }
    public List<RankDto> listRank() throws Exception {
        return rankRepository.listRank();
    }
}
